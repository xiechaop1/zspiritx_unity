using UnityEngine;

public class brach : MonoBehaviour
{
	public AudioClip Waterflush, Hit_jaw, Hit_head, Hit_tail, Largestep, Largesplash, Idleherb, Chew, Brach1, Brach2, Brach3, Brach4;
	Transform Head, Tail0, Tail1, Tail2, Tail3, Tail4, Tail5, Tail6, Tail7, Tail8, 
	Neck0, Neck1, Neck2, Neck3, Neck4, Neck5, Neck6, Neck7, Neck8, Neck9, Neck10, Neck11, Neck12, Neck13, Neck14, Neck15, Neck16, 
	Left_Arm0, Right_Arm0, Left_Arm1, Right_Arm1, Left_Hand, Right_Hand, 
	Left_Hips, Right_Hips, Left_Leg, Right_Leg, Left_Foot, Right_Foot;
	float crouch, spineX, spineY, tailX; bool reset;
	const float MAXYAW=10, MAXPITCH=8, MAXCROUCH=8, MAXANG=0.5f, ANGT=0.01f;

	Vector3 dir;
	shared shared;
	AudioSource[] source;
	Animator anm;
	Rigidbody body;

	//*************************************************************************************************************************************************
	//Get components
	void Start()
	{
		Left_Hips = transform.Find ("Brach/root/pelvis/left hips");
		Right_Hips = transform.Find ("Brach/root/pelvis/right hips");
		Left_Leg  = transform.Find ("Brach/root/pelvis/left hips/left leg");
		Right_Leg = transform.Find ("Brach/root/pelvis/right hips/right leg");

		Left_Foot = transform.Find ("Brach/root/pelvis/left hips/left leg/left foot");
		Right_Foot = transform.Find ("Brach/root/pelvis/right hips/right leg/right foot");

		Left_Arm0 = transform.Find ("Brach/root/spine0/spine1/spine2/spine3/spine4/left arm0");
		Right_Arm0 = transform.Find ("Brach/root/spine0/spine1/spine2/spine3/spine4/right arm0");
		Left_Arm1 = transform.Find ("Brach/root/spine0/spine1/spine2/spine3/spine4/left arm0/left arm1");
		Right_Arm1 = transform.Find ("Brach/root/spine0/spine1/spine2/spine3/spine4/right arm0/right arm1");

		Left_Hand = transform.Find ("Brach/root/spine0/spine1/spine2/spine3/spine4/left arm0/left arm1/left hand");
		Right_Hand = transform.Find ("Brach/root/spine0/spine1/spine2/spine3/spine4/right arm0/right arm1/right hand");

		Tail0 = transform.Find ("Brach/root/pelvis/tail0");
		Tail1 = transform.Find ("Brach/root/pelvis/tail0/tail1");
		Tail2 = transform.Find ("Brach/root/pelvis/tail0/tail1/tail2");
		Tail3 = transform.Find ("Brach/root/pelvis/tail0/tail1/tail2/tail3");
		Tail4 = transform.Find ("Brach/root/pelvis/tail0/tail1/tail2/tail3/tail4");
		Tail5 = transform.Find ("Brach/root/pelvis/tail0/tail1/tail2/tail3/tail4/tail5");
		Tail6 = transform.Find ("Brach/root/pelvis/tail0/tail1/tail2/tail3/tail4/tail5/tail6");
		Tail7 = transform.Find ("Brach/root/pelvis/tail0/tail1/tail2/tail3/tail4/tail5/tail6/tail7");
		Tail8 = transform.Find ("Brach/root/pelvis/tail0/tail1/tail2/tail3/tail4/tail5/tail6/tail7/tail8");

		Neck0 = transform.Find ("Brach/root/spine0/spine1/spine2/spine3/spine4/neck0");
		Neck1 = transform.Find ("Brach/root/spine0/spine1/spine2/spine3/spine4/neck0/neck1");
		Neck2 = transform.Find ("Brach/root/spine0/spine1/spine2/spine3/spine4/neck0/neck1/neck2");
		Neck3 = transform.Find ("Brach/root/spine0/spine1/spine2/spine3/spine4/neck0/neck1/neck2/neck3");
		Neck4 = transform.Find ("Brach/root/spine0/spine1/spine2/spine3/spine4/neck0/neck1/neck2/neck3/neck4");
		Neck5 = transform.Find ("Brach/root/spine0/spine1/spine2/spine3/spine4/neck0/neck1/neck2/neck3/neck4/neck5");
		Neck6 = transform.Find ("Brach/root/spine0/spine1/spine2/spine3/spine4/neck0/neck1/neck2/neck3/neck4/neck5/neck6");
		Neck7 = transform.Find ("Brach/root/spine0/spine1/spine2/spine3/spine4/neck0/neck1/neck2/neck3/neck4/neck5/neck6/neck7");
		Neck8 = transform.Find ("Brach/root/spine0/spine1/spine2/spine3/spine4/neck0/neck1/neck2/neck3/neck4/neck5/neck6/neck7/neck8");
		Neck9 = transform.Find ("Brach/root/spine0/spine1/spine2/spine3/spine4/neck0/neck1/neck2/neck3/neck4/neck5/neck6/neck7/neck8/neck9");
		Neck10 = transform.Find ("Brach/root/spine0/spine1/spine2/spine3/spine4/neck0/neck1/neck2/neck3/neck4/neck5/neck6/neck7/neck8/neck9/neck10");
		Neck11 = transform.Find ("Brach/root/spine0/spine1/spine2/spine3/spine4/neck0/neck1/neck2/neck3/neck4/neck5/neck6/neck7/neck8/neck9/neck10/neck11");
		Neck12 = transform.Find ("Brach/root/spine0/spine1/spine2/spine3/spine4/neck0/neck1/neck2/neck3/neck4/neck5/neck6/neck7/neck8/neck9/neck10/neck11/neck12");
		Neck13 = transform.Find ("Brach/root/spine0/spine1/spine2/spine3/spine4/neck0/neck1/neck2/neck3/neck4/neck5/neck6/neck7/neck8/neck9/neck10/neck11/neck12/neck13");
		Neck14 = transform.Find ("Brach/root/spine0/spine1/spine2/spine3/spine4/neck0/neck1/neck2/neck3/neck4/neck5/neck6/neck7/neck8/neck9/neck10/neck11/neck12/neck13/neck14");
		Neck15 = transform.Find ("Brach/root/spine0/spine1/spine2/spine3/spine4/neck0/neck1/neck2/neck3/neck4/neck5/neck6/neck7/neck8/neck9/neck10/neck11/neck12/neck13/neck14/neck15");
		Neck16 = transform.Find ("Brach/root/spine0/spine1/spine2/spine3/spine4/neck0/neck1/neck2/neck3/neck4/neck5/neck6/neck7/neck8/neck9/neck10/neck11/neck12/neck13/neck14/neck15/neck16");
		Head = transform.Find ("Brach/root/spine0/spine1/spine2/spine3/spine4/neck0/neck1/neck2/neck3/neck4/neck5/neck6/neck7/neck8/neck9/neck10/neck11/neck12/neck13/neck14/neck15/neck16/head");

		source = GetComponents<AudioSource>();
		shared= GetComponent<shared>();
		body=GetComponent<Rigidbody>();
		anm=GetComponent<Animator>();
	}

	//*************************************************************************************************************************************************
	//Play sound
	void OnCollisionStay(Collision col)
	{
		int rndPainsnd=Random.Range(0, 4); AudioClip painSnd=null;
		switch (rndPainsnd) { case 0: painSnd=Brach1; break; case 1: painSnd=Brach2; break; case 2: painSnd=Brach3; break; case 3: painSnd=Brach4; break; }
		shared.ManageCollision(col, MAXPITCH, MAXCROUCH, source, painSnd, Hit_jaw, Hit_head, Hit_tail);
	}
	void PlaySound(string name, int time)
	{
		if(time==shared.currframe && shared.lastframe!=shared.currframe)
		{
			switch (name)
			{
			case "Step": source[1].pitch=Random.Range(0.75f, 1.25f);
				if(shared.IsInWater) source[1].PlayOneShot(Waterflush, Random.Range(0.25f, 0.5f));
				else if(shared.IsOnWater) source[1].PlayOneShot(Largesplash, Random.Range(0.25f, 0.5f));
				else if(shared.IsOnGround) source[1].PlayOneShot(Largestep, Random.Range(0.25f, 0.5f));
				shared.lastframe=shared.currframe; break;
			case "Hit": source[1].pitch=Random.Range(1.0f, 1.25f); source[1].PlayOneShot(shared.IsOnWater|shared.IsInWater?Largesplash:Largestep, 1.5f);
				shared.lastframe=shared.currframe; break;
			case "Die": source[1].pitch=Random.Range(1.0f, 1.25f); source[1].PlayOneShot(shared.IsOnWater|shared.IsInWater?Largesplash:Largestep, 1.0f);
				shared.lastframe=shared.currframe; shared.IsDead=true; break;
			case "Chew": source[0].pitch=Random.Range(1.0f, 1.25f); source[0].PlayOneShot(Chew, 0.75f);
				shared.lastframe=shared.currframe; break;
			case "Sleep": source[0].pitch=Random.Range(1.0f, 1.25f); source[0].PlayOneShot(Idleherb, 0.25f);
				shared.lastframe=shared.currframe; break;
			case "Growl": int rnd = Random.Range (0, 4); source[0].pitch=Random.Range(1.0f, 1.25f);
				if(rnd==0)source[0].PlayOneShot(Brach1, 1.0f);
				else if(rnd==1)source[0].PlayOneShot(Brach2, 1.0f);
				else if(rnd==2)source[0].PlayOneShot(Brach3, 1.0f);
				else if(rnd==3)source[0].PlayOneShot(Brach4, 1.0f);
				shared.lastframe=shared.currframe; break;
			}
		}
	}
	
	//*************************************************************************************************************************************************
	// Add forces to the Rigidbody
	void FixedUpdate ()
	{
		if(!shared.IsActive) { body.Sleep(); return; }
		reset=false; shared.IsConstrained= false;
		AnimatorStateInfo CurrAnm=anm.GetCurrentAnimatorStateInfo(0);
		AnimatorStateInfo NextAnm=anm.GetNextAnimatorStateInfo(0);

		//Set mass
		if(shared.IsInWater) { body.mass=10; body.drag=1; body.angularDrag=1; }
		else { body.mass=1; body.drag=4; body.angularDrag=4; }
		//Set Y position
		if(shared.IsOnGround) //Ground
		{ dir=transform.forward; body.AddForce(Vector3.up*(shared.posY-transform.position.y)*64); }
		else if(shared.IsInWater | shared.IsOnWater) //Water
		{
			dir=transform.forward; body.AddForce(Vector3.up*(shared.posY-transform.position.y)*8);
			if(shared.Health>0) { anm.SetInteger ("Move", 2); shared.Health-=0.01f; }
		} else body.AddForce(-Vector3.up*Mathf.Lerp(dir.y, 128, 1.0f)); //Falling

		//Stopped
		if(NextAnm.IsName("Brach|IdleA") | CurrAnm.IsName("Brach|IdleA") | CurrAnm.IsName("Brach|Die"))
		{
			if(CurrAnm.IsName("Brach|Die")) { reset=true; shared.IsConstrained=true; if(!shared.IsDead) { PlaySound("Growl", 3); PlaySound("Die", 12); } }
		}
	
		//Forward
		else if(NextAnm.IsName("Brach|Walk") | CurrAnm.IsName("Brach|Walk") | CurrAnm.IsName("Brach|WalkGrowl"))
		{
			transform.rotation*= Quaternion.Euler(0, anm.GetFloat("Turn")*MAXANG, 0);
			body.AddForce(transform.forward*15*transform.localScale.x*anm.speed);
			if(anm.GetCurrentAnimatorStateInfo(0).IsName("Brach|WalkGrowl")) { PlaySound("Growl", 1); PlaySound("Step", 5); PlaySound("Step", 12); }
			else { PlaySound("Step", 5); PlaySound("Step", 12); }
		}

		//Run
		else if(NextAnm.IsName("Brach|Run") | CurrAnm.IsName("Brach|Run") | CurrAnm.IsName("Brach|RunGrowl"))
		{
			transform.rotation*= Quaternion.Euler(0, anm.GetFloat("Turn")*MAXANG, 0);
			body.AddForce(transform.forward*30*transform.localScale.x*anm.speed);
			if(anm.GetCurrentAnimatorStateInfo(0).IsName("Brach|RunGrowl")) { PlaySound("Growl", 2); PlaySound("Step", 5); PlaySound("Step", 12); }
			else { PlaySound("Step", 5); PlaySound("Step", 12); }
		}

		//Backward
		else if(NextAnm.IsName("Brach|Walk-") | CurrAnm.IsName("Brach|Walk-") | CurrAnm.IsName("Brach|WalkGrowl-"))
		{
			transform.rotation*= Quaternion.Euler(0, anm.GetFloat("Turn")*MAXANG, 0);
			body.AddForce(transform.forward*-15*transform.localScale.x*anm.speed);
			if(anm.GetCurrentAnimatorStateInfo(0).IsName("Brach|WalkGrowl-")) { PlaySound("Growl", 4); PlaySound("Step", 5); PlaySound("Step", 12); }
			else { PlaySound("Step", 5); PlaySound("Step", 12); }
		}

		//Strafe/Turn right
		else if(NextAnm.IsName("Brach|Strafe-") | CurrAnm.IsName("Brach|Strafe-"))
		{
			transform.rotation*= Quaternion.Euler(0, anm.GetFloat("Turn")*MAXANG, 0);
			body.AddForce(transform.right*5*transform.localScale.x*anm.speed);
			PlaySound("Step", 5); PlaySound("Step", 12);
		}

		//Strafe/Turn left
		else if(NextAnm.IsName("Brach|Strafe+") | CurrAnm.IsName("Brach|Strafe+"))
		{
			transform.rotation*= Quaternion.Euler(0, anm.GetFloat("Turn")*MAXANG, 0);
			body.AddForce(transform.right*-5*transform.localScale.x*anm.speed);
			PlaySound("Step", 5); PlaySound("Step", 12);
		}

		//Various
		else if(CurrAnm.IsName("Brach|EatA")) PlaySound("Chew", 10);
		else if(CurrAnm.IsName("Brach|EatB")) reset=true;
		else if(CurrAnm.IsName("Brach|EatC")) { reset=true; PlaySound("Chew", 1); PlaySound("Chew", 4); PlaySound("Chew", 8); PlaySound("Chew", 12); }
		else if(CurrAnm.IsName("Brach|ToSit")) shared.IsConstrained=true;
		else if(CurrAnm.IsName("Brach|ToSit-")) shared.IsConstrained=true;
		else if(CurrAnm.IsName("Brach|SitIdle")) shared.IsConstrained=true;
		else if(CurrAnm.IsName("Brach|Sleep") | CurrAnm.IsName("Brach|ToSleep") ) { reset=true; shared.IsConstrained=true; PlaySound("Sleep", 2); }
		else if(CurrAnm.IsName("Brach|SitGrowl")) { PlaySound("Growl", 7); shared.IsConstrained=true; }
		else if(CurrAnm.IsName("Brach|IdleB")) PlaySound("Growl", 2);
		else if(CurrAnm.IsName("Brach|RiseIdle")) reset=true;
		else if(CurrAnm.IsName("Brach|RiseGrowl")) { reset=true; PlaySound("Growl", 2); }
		else if(CurrAnm.IsName("Brach|ToRise")) { reset=true; PlaySound("Growl", 5); }
		else if(CurrAnm.IsName("Brach|ToRise-")) { PlaySound("Growl", 1); PlaySound("Hit", 4);}
		else if(CurrAnm.IsName("Brach|Die-")) { PlaySound("Growl", 3);  shared.IsDead=false; }
	}

	void LateUpdate()
	{
		//*************************************************************************************************************************************************
		// Bone rotation
		if(!shared.IsActive) return;

		//Set const varialbes to shared script
		shared.crouch_max=MAXCROUCH;
		shared.ang_t=ANGT;
		shared.yaw_max=MAXYAW;
		shared.pitch_max=MAXPITCH;

		if(shared.lastHit!=0)	//Taking damage animation
		{
			crouch=Mathf.Lerp(crouch, (MAXCROUCH*transform.localScale.x)/2, 1.0f);
			shared.lastHit--; Head.GetChild(0).transform.rotation*= Quaternion.Euler(-shared.lastHit, 0, 0);
		}
		else if(reset) //Reset
		{
			anm.SetFloat("Turn", Mathf.Lerp(anm.GetFloat("Turn"), 0.0f, ANGT*3));
			spineX=Mathf.Lerp(spineX, 0.0f, ANGT);
			spineY=Mathf.Lerp(spineY, 0.0f, ANGT);
			crouch=Mathf.Lerp(crouch, 0, ANGT);
		}
		else
		{
			shared.TargetLooking(spineX, spineY,crouch);
			spineX=Mathf.Lerp(spineX, shared.spineX_T, ANGT);
			spineY=Mathf.Lerp(spineY, shared.spineY_T, ANGT);
			crouch=Mathf.Lerp(crouch, shared.crouch_T, ANGT);
		}

		//Save head position befor transformation
		shared.fixedHeadPos=Head.position;
	
		//Spine rotation
		float spineZ =spineY*spineX/MAXYAW;
		Neck0.transform.rotation*= Quaternion.Euler(0, 0, -spineX);
		Neck1.transform.rotation*= Quaternion.Euler(0, 0, -spineX);
		Neck2.transform.rotation*= Quaternion.Euler(0, 0, -spineX);
		Neck3.transform.rotation*= Quaternion.Euler(0, 0, -spineX);
		Neck4.transform.rotation*= Quaternion.Euler(0, 0, -spineX);
		Neck5.transform.rotation*= Quaternion.Euler(0, 0, -spineX);
		Neck6.transform.rotation*= Quaternion.Euler(0, 0, -spineX);
		Neck7.transform.rotation*= Quaternion.Euler(0, 0, -spineX);
		Neck8.transform.rotation*= Quaternion.Euler(spineY, 0, 0);
		Neck9.transform.rotation*= Quaternion.Euler(spineY, 0, 0);
		Neck10.transform.rotation*= Quaternion.Euler(spineY, 0, 0);
		Neck11.transform.rotation*= Quaternion.Euler(spineY, 0, 0);
		Neck12.transform.rotation*= Quaternion.Euler(spineY, 0, 0);
		Neck13.transform.rotation*= Quaternion.Euler(spineY, 0, 0);
		Neck14.transform.rotation*= Quaternion.Euler(spineY, 0, 0);
		Neck15.transform.rotation*= Quaternion.Euler(spineY, spineZ, -spineX);
		Neck16.transform.rotation*= Quaternion.Euler(spineY, spineZ, -spineX);
		Head.transform.rotation*= Quaternion.Euler(spineY, spineZ, -spineX);

		//Tail rotation
		tailX=Mathf.Lerp(tailX, anm.GetFloat("Turn")*MAXYAW, ANGT*3);
		Tail0.transform.rotation*= Quaternion.Euler(0, 0, tailX);
		Tail1.transform.rotation*= Quaternion.Euler(0, 0, tailX);
		Tail2.transform.rotation*= Quaternion.Euler(0, 0, tailX);
		Tail3.transform.rotation*= Quaternion.Euler(0, 0, tailX);
		Tail4.transform.rotation*= Quaternion.Euler(0, 0, tailX);
		Tail5.transform.rotation*= Quaternion.Euler(0, 0, tailX);
		Tail6.transform.rotation*= Quaternion.Euler(0, 0, tailX);
		Tail7.transform.rotation*= Quaternion.Euler(0, 0, tailX);
		Tail8.transform.rotation*= Quaternion.Euler(0, 0, tailX);

		//IK feet (require "JP script extension" asset)
		//shared.QuadIK( Right_Arm0, Right_Arm1, Right_Hand, Left_Arm0, Left_Arm1, Left_Hand, Right_Hips, Right_Leg, Right_Foot, Left_Hips, Left_Leg, Left_Foot, -0.6f*transform.localScale.x);
		//Check for ground layer
		//shared.GetGroundAlt(true, crouch);

		//*************************************************************************************************************************************************
		// CPU (require "JP script extension" asset)
		//if(shared.AI && shared.Health!=0) { shared.AICore(1, 4, 0, 0, 2, 3, 5); }
		//*************************************************************************************************************************************************
		// Human
		//else if(shared.Health!=0) { shared.GetUserInputs(1, 4, 0, 0, 2, 3, 5, 4); }
		//*************************************************************************************************************************************************
		//Dead
		//else { anm.SetInteger ("Move", 0); anm.SetInteger ("Idle", -1); }
	}
}