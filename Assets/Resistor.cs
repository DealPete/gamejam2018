﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resistor : Wire {
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	public override void DoAction(Agent agent){
		agent.damage (1);
	}
}
