using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Energy is low or not", story: "Energy is below 20", category: "Conditions", id: "f4208e1413fba29f08936c841d3a7ee4")]
public partial class EnergyIsLowOrNotCondition : Condition
{

    public override bool IsTrue()
    {
        return true;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
