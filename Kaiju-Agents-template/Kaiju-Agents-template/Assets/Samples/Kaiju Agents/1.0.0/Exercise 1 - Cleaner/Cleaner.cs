using System.Collections.Generic;
using KaijuSolutions.Agents;
using KaijuSolutions.Agents.Actuators;
using KaijuSolutions.Agents.Exercises.Cleaner;
using KaijuSolutions.Agents.Extensions;
using KaijuSolutions.Agents.Movement;
using KaijuSolutions.Agents.Sensors;
using UnityEngine;

public class Cleaner : KaijuController
{
    
    private enum CleanerStates
    {
        RandomLooking,
        Cleaning,
        Roomba,
        SeekingToTarget,
        SeekingToPoint
    }

    private CleanerStates _state = CleanerStates.Roomba;
    
    private Floor _nearest;

    private GameObject _tiles;

    private KaijuAttackActuator _actuator;
    
    public List<Transform> points;
    
    private KaijuEverythingVisionSensor _sensor;

    private Transform nearestPoint;

    private Transform _nextPoint;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _sensor = Agent.GetSensor<KaijuEverythingVisionSensor>();
        _actuator =  Agent.GetActuator<CleanerActuator>();
        _tiles = GameObject.Find("Path");
        for (int i = 0; i < 21; i+=1)
        {
            points.Add(_tiles.transform.GetChild(i));
        }
        
        SeekToClosestPoint();
        
        
    }

    protected override void OnSense(KaijuSensor sensor)
    {
        if (_sensor.ObservedCount < 1)
        {
            return;
        }
        
        // _sensor.Observed
        _nearest = (Floor)Position.Nearest(_sensor.Observed, out float _);

        if (_nearest.Dirty)
        {
            _state = CleanerStates.SeekingToTarget;
            Agent.Seek(_nearest.transform, 1f);
        }
    }

    private void SeekToClosestPoint()
    {
        nearestPoint = Position.Nearest(points, out float _);
        
        // _nextPoint = nearestPoint;
        _state = CleanerStates.SeekingToPoint; 
        Agent.Seek(nearestPoint, 1f);
    }   


    private void Roomba()
    {
        if (_nextPoint == null)
        {
            _nextPoint = nearestPoint;
        }
        int index = points.IndexOf(_nextPoint);
        index = (index + 1) % points.Count;
        _nextPoint = points[index];
        

        // _state = CleanerStates.Roomba;
        Agent.Seek(_nextPoint,1f);
        print(nearestPoint.name);
        
    }
    
    
    private void StartLooking()
    {
        _state = CleanerStates.RandomLooking;
        _sensor.automatic = true;
        Agent.Wander();
        Agent.ObstacleAvoidance(clear: false);
    }

    protected override void OnMovementStopped(KaijuMovement movement)
    {
        
        
        if (movement is KaijuSeekMovement && _state == CleanerStates.SeekingToTarget)
        {
            _state = CleanerStates.Cleaning;
            _actuator.Begin();
        }
        else if (movement is KaijuSeekMovement &&  _state == CleanerStates.SeekingToPoint)
        {
            _state = CleanerStates.Roomba;
            Roomba();
        }
        else if (movement is KaijuSeekMovement && _state == CleanerStates.Roomba)
        {
            if (_nextPoint.Within(Position3, 1f))
            {
                Roomba();
            }
        }
        
    }

    protected override void OnActuatorDone(KaijuActuator actuator)
    {
        SeekToClosestPoint();
    }

    protected override void OnActuatorFailed(KaijuActuator actuator)
    {
        Debug.Log("Actuator failed");
        SeekToClosestPoint();
    }
}
