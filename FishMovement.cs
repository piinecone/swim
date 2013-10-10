﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FishMovement : MonoBehaviour {
  [SerializeField]
  private List<GameObject> waypoints;
  [SerializeField]
  private float forwardSpeed = 25f;
  [SerializeField]
  private float burstSpeed = 40f;
  [SerializeField]
  private float fastRotationSpeed = .75f;
  [SerializeField]
  private float obstacleAvoidanceRotationSpeed = 1f;
  [SerializeField]
  private float quickChangeOfDirectionDistance = 2f;
  [SerializeField]
  public bool isLeadFish = false;
  [SerializeField]
  private SchoolOfFishMovement schoolOfFish;

  private Transform nextWaypoint;
  private Transform lastWaypoint;
  private int nextWaypointIndex;
  private GameObject leadFish;
  private Vector3 leadFishOffset;
  private float leadFishDistance;
  private Vector3 adjustedTarget;
  private float burstTimer = 1.5f;
  private float timeleft = 0;
  private float currentBurstSpeed;

  void Start () {
  }
  
  void Update () {
    if (needsNewWaypoint()){
      determineNextWaypoint();
    }
    moveTowardNextWaypoint();
  }

  private void mimicLeadFish(){
    if (leadFish != null){
      Vector3 targetPosition = leadFish.transform.position - leadFishOffset;
      Vector3 direction = directionAfterAvoidingObstacles(targetPosition);
      moveInDirection(targetPosition, direction);
    }
  }

  private void moveTowardNextWaypoint(){
    Vector3 targetPosition = nextWaypoint.position - leadFishOffset;
    Vector3 direction = directionAfterAvoidingObstacles(targetPosition);
    moveInDirection(targetPosition, direction);
  }

  private void moveInDirection(Vector3 targetPosition, Vector3 direction){
    Quaternion rotation = Quaternion.LookRotation(direction);
    if (burstToNextWaypoint(false)){
      smoothlyLookAtNextWaypoint();
      transform.position += transform.forward * currentBurstSpeed * Time.deltaTime;
    } else {
      transform.rotation = Quaternion.Slerp(transform.rotation, rotation, obstacleAvoidanceRotationSpeed * Time.deltaTime);
      transform.position += transform.forward * forwardSpeed * Time.deltaTime;
    }
  }

  private void smoothlyLookAtNextWaypoint(){
    Vector3 targetPosition = nextWaypoint.position - leadFishOffset;
    Vector3 direction = (targetPosition - transform.position).normalized;
    Quaternion rotation = Quaternion.LookRotation(direction);
    transform.rotation = Quaternion.Slerp(transform.rotation, rotation, fastRotationSpeed * Time.deltaTime);
  }

  private Vector3 directionAfterAvoidingObstacles(Vector3 targetPosition){
    RaycastHit hit;
    Vector3 direction = (targetPosition - transform.position).normalized;
    float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
    float sensoryDistance = 10f;
    float hitSensitivity = 50f;

    Vector3 forwardRay = transform.forward * distanceToTarget;
    Vector3 leftRay = transform.forward * distanceToTarget;
    leftRay.x -= .5f;
    Vector3 rightRay = transform.forward * distanceToTarget;
    rightRay.x += .5f;

    if (Physics.Raycast(transform.position, forwardRay, out hit, sensoryDistance)){
      if (hit.transform != transform){
        direction += hit.normal * hitSensitivity;
      }
    }
    if (Physics.Raycast(transform.position, leftRay, out hit, sensoryDistance)){
      if (hit.transform != transform){
        direction += hit.normal * hitSensitivity;
      }
    }
    if (Physics.Raycast(transform.position, rightRay, out hit, sensoryDistance)){
      if (hit.transform != transform){
        direction += hit.normal * hitSensitivity;
      }
    }

    return direction;
  }

  private bool justPassedWaypoint(){
    if (lastWaypoint != null && Vector3.Distance(transform.position, (lastWaypoint.position - leadFishOffset)) < 10f){
      return true;
    } else {
      return false;
    }
  }

  private bool needsNewWaypoint(){
    if (Vector3.Distance(transform.position, nextWaypoint.position - leadFishOffset) < .5f){
      return true;
    } else {
      return false;
    }
  }

  private void determineNextWaypoint(){
    nextWaypointIndex++;
    if (waypoints.Count <= nextWaypointIndex){
      nextWaypointIndex = 0;
    }
    setNextWaypoint(nextWaypointIndex);
    schoolOfFish.BroadcastNextWaypoint(nextWaypointIndex);
  }

  public Vector3 LerpByDistance(Vector3 from, Vector3 target, float distance){
    return (distance * Vector3.Normalize(target - from) + from);
  }

  public void setNextWaypoint(int index){
    nextWaypointIndex = index;
    lastWaypoint = nextWaypoint;
    nextWaypoint = waypoints[nextWaypointIndex].transform;
  }

  public void setLeadFish(GameObject fish){
    leadFish = fish;
    leadFishOffset = leadFish.transform.position - transform.position;
    leadFishDistance = Vector3.Distance(transform.position, leadFish.transform.position);
  }

  public bool burstToNextWaypoint(bool start){
    timeleft = start ? burstTimer : (timeleft -= Time.deltaTime);
    if (timeleft > 0f){
      currentBurstSpeed = (timeleft > (burstTimer * .8f)) ? burstSpeed : forwardSpeed;
      return true;
    } else {
      return false;
    }
  }
}
