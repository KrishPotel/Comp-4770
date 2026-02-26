using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using KaijuSolutions.Agents;
using KaijuSolutions.Agents.Actuators;
using KaijuSolutions.Agents.Exercises.Cleaner;
using KaijuSolutions.Agents.Sensors;
using UnityEngine;

public class CleanerActuator : KaijuAttackActuator
{
    protected override bool HandleHit(RaycastHit hit, Transform t)
    {
        if (!t.name.StartsWith("Floor"))
        {
            return false;
            
        }

        t.gameObject.GetComponents<Floor>()[0].Clean();
        
        return true;
    }
}
