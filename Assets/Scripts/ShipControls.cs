﻿using UnityEngine;
using System.Collections;

public class ShipControls : MonoBehaviour {
	private float rotationSpeed = 40f;
	private float torqueSpeed = 1.5f;
	private float acceleration = 5f;
	private float thrust = 0.0f;
	private float maxThrust = 250.0f;
	private float maxVelocity = 5.5f;
	private float thrusterSpeed = 10f;
	private float survivableImpactForce = 1500f;
	private int minedOre = 0;
	private float miniMapWidth = 0.15f;
	private float miniMapHeight;

	private bool alive = true;

	public Scanner scanner;
	public MiningBeam miningBeam;
	public GameObject body;
	public GameObject leftThruster;
	public GameObject rightThruster;
	public GameObject frontLeftExhaust;
	public GameObject backLeftExhaust;
	public GameObject frontRightExhaust;
	public GameObject backRightExhaust;
	public GameObject explosion;
	public Texture miniMapBackground;
	public Camera miniMapCamera;
	public GameObject pressToContinue;
	public GameObject logo;
	public GameObject instructions;
	public Texture oreStack;
	public Font font;

	void Start(){

		// Set sort order of particle systems

		explosion.particleSystem.renderer.sortingLayerName = "E";

		// Set size / position of minimap camera

		miniMapHeight = miniMapWidth * miniMapCamera.aspect;
		miniMapCamera.rect = new Rect (1f - miniMapWidth, 0f, miniMapWidth, miniMapHeight);
	}

	void Update(){
		// Hide logo / instructions

		if(Input.anyKeyDown){
			logo.BroadcastMessage("Hide");
			instructions.BroadcastMessage("Hide");
		}

		if(alive){

			// Control angular thrust

			body.transform.Rotate (Vector3.forward * (-Input.GetAxis("Horizontal") * thrust / maxThrust * rotationSpeed * Time.deltaTime) ); 
			// rigidbody2D.AddTorque(-Input.GetAxis("Horizontal") * thrust * torqueSpeed * Time.deltaTime);

			// Control velocity

			float inputThrust = Input.GetAxis("Vertical");
			thrust = Mathf.Clamp(thrust + inputThrust * acceleration, -maxThrust, maxThrust);

			Vector3 newForce = Vector3.up * thrust * Time.deltaTime;
			Vector3 relativeForce = body.transform.InverseTransformDirection(newForce);
			rigidbody2D.AddForce(new Vector2(relativeForce.x * -1f, relativeForce.y));

			rigidbody2D.velocity = Vector3.ClampMagnitude(rigidbody2D.velocity, maxVelocity);

			// Control scanner

			if(Input.GetMouseButtonDown(0) || Input.GetKeyDown("left shift") || Input.GetKeyDown("right shift")){
				scanner.Scan(transform.position);
			}

			// Control mining beam

			// TODO: Only when landed

			if(Input.GetMouseButtonDown(1) || Input.GetKeyDown("space")){
				miningBeam.Mine();
			}

			if(Input.GetMouseButtonUp(1) || Input.GetKeyUp("space")){
				miningBeam.Stop();
			}
		}else{

			// Restart game

			if(Input.anyKey){
				Application.LoadLevel(Application.loadedLevel);
			}

		}

		// Animate thrusters
		
		float targetThrusterAngle = 0f;
		
		if(Input.GetAxis("Horizontal") > 0f){
			targetThrusterAngle = 360f - 15f;
		}else if(Input.GetAxis("Horizontal") < 0){
			targetThrusterAngle = 15f;
		}
		
		// Debug.Log (targetThrusterAngle + ":" + leftThruster.transform.localEulerAngles.z);
		
		Quaternion targetThrusterRotation = Quaternion.Euler(new Vector3 (0f, 0f, targetThrusterAngle));
		
		leftThruster.transform.localRotation = Quaternion.Lerp (leftThruster.transform.localRotation, targetThrusterRotation, thrusterSpeed * Time.deltaTime);
		rightThruster.transform.localRotation = Quaternion.Lerp (rightThruster.transform.localRotation, targetThrusterRotation, thrusterSpeed * Time.deltaTime);	
		
		// Animate exhaust
		
		Vector3 backLeftPos = backLeftExhaust.transform.localPosition;
		Vector3 backRightPos = backRightExhaust.transform.localPosition;
		Vector3 frontLeftPos = frontLeftExhaust.transform.localPosition;
		Vector3 frontRightPos = frontRightExhaust.transform.localPosition;
		
		if(thrust > 0f){
			backLeftPos.y = -0.25f * thrust / maxThrust;
			backRightPos.y = backLeftPos.y;
			frontLeftPos.y = 0f;
			frontRightPos.y = 0f;
		}else{
			frontLeftPos.y = -0.25f * thrust / maxThrust;
			frontRightPos.y = frontLeftPos.y;
			backLeftPos.y = 0f;
			backRightPos.y = 0f;
		}
		
		backLeftExhaust.transform.localPosition = backLeftPos;
		backRightExhaust.transform.localPosition = backRightPos;
		frontLeftExhaust.transform.localPosition = frontLeftPos;
		frontRightExhaust.transform.localPosition = frontRightPos;
	}

	void MinedOre(){
		minedOre ++;
	}

	void OnGUI(){
		// GUI.Label(new Rect(10, 10, 100, 25), "Thrust: " + thrust);
		// GUI.Label(new Rect(10, 25, 100, 25), "Ore: " + minedOre);

		GUI.DrawTexture(new Rect(10, Screen.height - 110, 50, 100), oreStack);

		GUIStyle scoreStyle = new GUIStyle ();
		scoreStyle.font = font;
		scoreStyle.fontSize = 30;
		scoreStyle.normal.textColor = new Color (255f, 255f, 255f, 0.75f);
		GUI.Label(new Rect(75, Screen.height - 80, 200, 30),  minedOre.ToString() + " ORE COLLECTED", scoreStyle);

		GUIStyle hiScoreStyle = new GUIStyle ();
		hiScoreStyle.font = font;
		hiScoreStyle.fontSize = 18;
		hiScoreStyle.normal.textColor = new Color (255f, 255f, 255f, 0.6f);
		GUI.Label (new Rect (75, Screen.height - 50, 200, 20), PlayerPrefs.GetInt ("HiScore").ToString () + " HI SCORE", hiScoreStyle);

		float mapWidth = Screen.width * miniMapWidth;
		float mapHeight = Screen.height * miniMapHeight;
		GUI.DrawTexture(new Rect(Screen.width - mapWidth, Screen.height - mapHeight, mapWidth, mapHeight), miniMapBackground, ScaleMode.ScaleToFit, true, 1f);

		miniMapCamera.Render();
	}

	void OnCollisionEnter2D(Collision2D collision){
		float impact = rigidbody2D.velocity.x - collision.gameObject.rigidbody2D.velocity.x;
		impact += rigidbody2D.velocity.y - collision.gameObject.rigidbody2D.velocity.y;
		impact *= rigidbody2D.mass * collision.gameObject.rigidbody2D.mass;
		impact = Mathf.Abs(impact);

		if(impact > survivableImpactForce && alive){
			alive = false;
			thrust = 0.0f;
			Destroy(miningBeam.gameObject);

			PlayerPrefs.SetInt("HiScore", minedOre);

			pressToContinue.BroadcastMessage("Show");
			logo.BroadcastMessage("Hide");

			explosion.particleSystem.Play();

			GameObject[] components = new GameObject[]{
				leftThruster,
				rightThruster,
				body
			};

			foreach(GameObject g in components){
				g.transform.parent = null;

				Rigidbody2D r = g.GetComponent<Rigidbody2D>();
				if(!r){
					r = g.AddComponent<Rigidbody2D>();
				}
				r.gravityScale = 0f;
				r.mass = 1f;
				r.AddForce(Random.insideUnitSphere * 10f);
				r.AddTorque(Random.Range(-500f, 500f));
			}
		}else{
			thrust = 0.0f;
		}
	}
}
