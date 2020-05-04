using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedSliderLabel : SliderLabel
{
    protected override void OnValueChanged( float a_value ) {
        var level = Mathf.FloorToInt( a_value );
        Text = Utility.GameData.GetSpeedData( level ).name;
    }
}
