using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RatScript : MonoBehaviour
{
    public int id;
    public Rigidbody rb;
    public RatManager rms;
    public GameObject rm;
    public Transform ratTransform;
    public NavMeshAgent agent;
    public Animator anim;

    public bool newSpawned;
    public bool alive;
    public float hunger;
    public float energy;
    public float HP;

    public float hungerRate;
    public float energyRate;
    public float HungerHPRate;
    public float hungerRecoverRate;
    public float energyRecoverRate;
    public float energyHPRecoverRate;

    private RatBaseState currentState;

    public readonly RatIdleState IdleState = new RatIdleState();
    public readonly RatLookingForFoodState LookingForFoodState = new RatLookingForFoodState();
    public readonly RatEatingState EatingState = new RatEatingState();
    public readonly RatSleepingState SleepingState = new RatSleepingState();
    public readonly RatDeathState DeathState = new RatDeathState();
    public readonly RatSpawnState SpawnState = new RatSpawnState();

    public int[,] foodMap;

    void Start()
    {
        Debug.Log("Rat start: " + this.transform.position);
        agent.enabled = false;
        rb = GetComponent<Rigidbody>();
        ratTransform = GetComponent<Transform>();

        rm = GameObject.Find("RatManager");
        rms = rm.GetComponent<RatManager>();

        alive = true;
        newSpawned = true;
        HP = 1.0f;
        hunger = 1.0f;
        energy = 1.0f;

        hungerRate = 0.02f;
        energyRate = hungerRate/4;
        hungerRecoverRate = hungerRate*10f;
        energyRecoverRate = energyRate*10f;
        energyHPRecoverRate = energyRecoverRate;
        HungerHPRate = energyHPRecoverRate/2f;

        TransitionToState(SpawnState);
        
    }

    // Update is called once per frame
    void Update()
    {
        if(ratTransform.position.z>0){
            //Time.timeScale = 0;
        }
        if(!newSpawned && alive){
            ratRoutine();
        }
        
        currentState.Update(this);
    }

    public void objectDropped(){
        Vector3 pos = this.transform.position;
        Debug.Log("Rat Dropped");
        if(newSpawned){
            rb.isKinematic = false;
            this.transform.parent = rm.transform;
            rms.removeRatTrainCell(id);
            
        }
    }

    public void OnCollisionEnter(Collision other){
        Debug.Log("collision: " + other);
        currentState.OnCollisionEnter(this, other);
    }

    private void ratRoutine(){
        hunger = hunger > 0f ? hunger - hungerRate * Time.deltaTime: 0f;     
        energy = energy > 0f ? energy - energyRate * Time.deltaTime: 0f;
        HealthChangeByHunger();

        if(HP <= 0){
            TransitionToState(DeathState);
        }
    }

    private void HealthChangeByHunger(){
        if(hunger<0.3f){
            HP = HP>0f? HP - HungerHPRate * Time.deltaTime : 0f;
        }
    }

    public void SetID(int i){
        id = i;
    }

    public void TransitionToState(RatBaseState state){
        currentState = state;
        currentState.EnterState(this);
    }

    public RatBaseState CurrentState{
        get {return currentState; }
    }

    public void DownloadFoodMap(int[,] map){
        foodMap = map;
        Debug.Log(foodMap[1,1]);

    }

    public void UploadFoodMap(){
        rms.UpdateFoodMap(this.foodMap);
    }

    public int[,] getFoodMap(){
        return this.foodMap;
    }

    public void die(){
        StopRunning();
        agent.enabled = false;
        rb.constraints = RigidbodyConstraints.FreezeRotationZ;
        ratTransform.RotateAround(ratTransform.position,new Vector3(0f, 0f, 1f), 90);
        alive = false;
    }

    public void RunToDestination(Vector3 des){
        agent.SetDestination(des);
        anim.CrossFade("Running", 0.1f);
        anim.SetBool("IsIdle", false);
    }

    public void StopRunning(){
        if(agent.enabled){
            agent.ResetPath();
        }
        anim.CrossFade("Idle", 0.1f);
        anim.SetBool("IsIdle", true);
    }
}