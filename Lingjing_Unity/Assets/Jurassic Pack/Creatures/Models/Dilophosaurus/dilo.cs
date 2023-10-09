using UnityEngine;

public class dilo : MonoBehaviour
{
	public AudioClip Waterflush, Hit_jaw, Hit_head, Hit_tail, Smallstep, Smallsplash, Idlecarn, Swallow, Bite, Dilo1, Dilo2, Dilo3, Dilo4, Dilo5, Dilo6;
	Transform Spine0, Spine1, Spine2, Spine3, Spine4, Spine5, Neck0, Neck1, Neck2, Neck3, Head, 
	Tail0, Tail1, Tail2, Tail3, Tail4, Tail5, Tail6, Tail7, Tail8, Tail9, Tail10, Tail11, Arm1, Arm2, 
	Left_Hips, Right_Hips, Left_Leg, Right_Leg, Left_Foot0, Right_Foot0, Left_Foot1, Right_Foot1;
	float crouch, spineX, spineY, tailX; bool reset;
	const float MAXYAW=16, MAXPITCH=9, MAXCROUCH=2, MAXANG=4, ANGT=0.15f;

	Vector3 dir;
	shared shared;
	AudioSource[] source;
	Animator anm;
	Rigidbody body;

	//*************************************************************************************************************************************************
	//Get components
	void Start()
	{
		Right_Hips = transform.Find ("Dilo/root/pelvis/right leg0");
		Right_Leg = transform.Find ("Dilo/root/pelvis/right leg0/right leg1");
		Right_Foot0 = transform.Find ("Dilo/root/pelvis/right leg0/right leg1/right foot0");
		Right_Foot1 = transform.Find ("Dilo/root/pelvis/right leg0/right leg1/right foot0/right foot1");
		Left_Hips = transform.Find ("Dilo/root/pelvis/left leg0");
		Left_Leg = transform.Find ("Dilo/root/pelvis/left leg0/left leg1");
		Left_Foot0 = transform.Find ("Dilo/root/pelvis/left leg0/left leg1/left foot0");
		Left_Foot1 = transform.Find ("Dilo/root/pelvis/left leg0/left leg1/left foot0/left foot1");
		
		Tail0 = transform.Find ("Dilo/root/pelvis/tail0");
		Tail1 = transform.Find ("Dilo/root/pelvis/tail0/tail1");
		Tail2 = transform.Find ("Dilo/root/pelvis/tail0/tail1/tail2");
		Tail3 = transform.Find ("Dilo/root/pelvis/tail0/tail1/tail2/tail3");
		Tail4 = transform.Find ("Dilo/root/pelvis/tail0/tail1/tail2/tail3/tail4");
		Tail5 = transform.Find ("Dilo/root/pelvis/tail0/tail1/tail2/tail3/tail4/tail5");
		Tail6 = transform.Find ("Dilo/root/pelvis/tail0/tail1/tail2/tail3/tail4/tail5/tail6");
		Tail7 = transform.Find ("Dilo/root/pelvis/tail0/tail1/tail2/tail3/tail4/tail5/tail6/tail7");
		Tail8 = transform.Find ("Dilo/root/pelvis/tail0/tail1/tail2/tail3/tail4/tail5/tail6/tail7/tail8");
		Tail9 = transform.Find ("Dilo/root/pelvis/tail0/tail1/tail2/tail3/tail4/tail5/tail6/tail7/tail8/tail9");
		Tail10 = transform.Find ("Dilo/root/pelvis/tail0/tail1/tail2/tail3/tail4/tail5/tail6/tail7/tail8/tail9/tail10");
		Tail11 = transform.Find ("Dilo/root/pelvis/tail0/tail1/tail2/tail3/tail4/tail5/tail6/tail7/tail8/tail9/tail10/tail11");
		Spine0 = transform.Find ("Dilo/root/spine0");
		Spine1 = transform.Find ("Dilo/root/spine0/spine1");
		Spine2 = transform.Find ("Dilo/root/spine0/spine1/spine2");
		Spine3 = transform.Find ("Dilo/root/spine0/spine1/spine2/spine3");
		Spine4 = transform.Find ("Dilo/root/spine0/spine1/spine2/spine3/spine4");
		Spine5 = transform.Find ("Dilo/root/spine0/spine1/spine2/spine3/spine4/spine5");
		Arm1  = transform.Find ("Dilo/root/spine0/spine1/spine2/spine3/spine4/spine5/left arm0");
		Arm2  = transform.Find ("Dilo/root/spine0/spine1/spine2/spine3/spine4/spine5/right arm0");
		Neck0 = transform.Find ("Dilo/root/spine0/spine1/spine2/spine3/spine4/spine5/neck0");
		Neck1 = transform.Find ("Dilo/root/spine0/spine1/spine2/spine3/spine4/spine5/neck0/neck1");
		Neck2 = transform.Find ("Dilo/root/spine0/spine1/spine2/spine3/spine4/spine5/neck0/neck1/neck2");
		Neck3 = transform.Find ("Dilo/root/spine0/spine1/spine2/spine3/spine4/spine5/neck0/neck1/neck2/neck3");
		Head  = transform.Find ("Dilo/root/spine0/spine1/spine2/spine3/spine4/spine5/neck0/neck1/neck2/neck3/head");
		
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
		switch (rndPainsnd) { case 0: painSnd=Dilo3; break; case 1: painSnd=Dilo4; break; case 2: painSnd=Dilo5; break; case 3: painSnd=Dilo6; break; }
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
				else if(shared.IsOnWater) source[1].PlayOneShot(Smallsplash, Random.Range(0.25f, 0.5f));
				else if(shared.IsOnGround) source[1].PlayOneShot(Smallstep, Random.Range(0.25f, 0.5f));
				shared.lastframe=shared.currframe; break;
			case "Bite": source[1].pitch=Random.Range(1.0f, 1.25f); source[1].PlayOneShot(Bite, 0.5f);
				shared.lastframe=shared.currframe; break;
			case "Die": source[1].pitch=Random.Range(0.8f, 1.0f); source[1].PlayOneShot(shared.IsOnWater|shared.IsInWater?Smallsplash:Smallstep, 1.0f);
				shared.lastframe=shared.currframe; shared.IsDead=true; break;
			case "Food": source[0].pitch=Random.Range(3.0f, 3.5f); source[0].PlayOneShot(Swallow, 0.025f);
				shared.lastframe=shared.currframe; break;
			case "Sleep": source[0].pitch=Random.Range(3.0f, 3.5f); source[0].PlayOneShot(Idlecarn, 0.25f);
				shared.lastframe=shared.currframe; break;
			case "AtkA": int rnd3 = Random.Range (0, 2); source[0].pitch=Random.Range(1.0f, 1.25f);
				if(rnd3==0)source[0].PlayOneShot(Dilo4, 1.0f);
				else if(rnd3==1)source[0].PlayOneShot(Dilo5, 1.0f);
				shared.lastframe=shared.currframe; break;
			case "AtkB": int rnd2 = Random.Range (0, 2); source[0].pitch=Random.Range(1.0f, 1.25f);
				if(rnd2==0)source[0].PlayOneShot(Dilo3, 1.0f);
				else if(rnd2==1)source[0].PlayOneShot(Dilo6, 1.0f);
				shared.lastframe=shared.currframe; break;
			case "Growl": int rnd1 = Random.Range (0, 2); source[0].pitch=Random.Range(1.0f, 1.25f);
				if(rnd1==0)source[0].PlayOneShot(Dilo1, 1.0f);
				else if(rnd1==1)source[0].PlayOneShot(Dilo2, 1.0f);
				shared.lastframe=shared.currframe; break;
			}
		}
	}

	//*************************************************************************************************************************************************
	// Add forces to the Rigidbody
	void FixedUpdate()
	{
		if(!shared.IsActive) { body.Sleep(); return; }
		reset=false; shared.IsAttacking=false; shared.IsJumping=false; shared.IsConstrained= false;
		AnimatorStateInfo CurrAnm=anm.GetCurrentAnimatorStateInfo(0);
		AnimatorStateInfo NextAnm=anm.GetNextAnimatorStateInfo(0);

		//Set mass
		if(shared.IsInWater) { body.mass=10; body.drag=1; body.angularDrag=1; }
		else { body.mass=1; body.drag=4; body.angularDrag=4; }
		//Set Y position
		if(shared.IsOnGround) //Ground
		{ dir=transform.forward; body.AddForce(Vector3.up*(shared.posY-transform.position.y)*64); anm.SetBool("OnGround", true); }
		else if(shared.IsInWater | shared.IsOnWater) //Water
		{
			dir=transform.forward; body.AddForce(Vector3.up*(shared.posY-transform.position.y)*8);
			if(shared.Health>0) { anm.SetInteger ("Move", 2); shared.Health-=0.01f; }
		} else { body.AddForce(-Vector3.up*Mathf.Lerp(dir.y, 128, 0.5f)); anm.SetBool("OnGround", false); }//Falling

		//Stopped
		if(NextAnm.IsName("Dilo|IdleA") | CurrAnm.IsName("Dilo|IdleA") | CurrAnm.IsName("Dilo|Die"))
		{
			if(CurrAnm.IsName("Dilo|Die")) { reset=true; shared.IsConstrained=true; if(!shared.IsDead) { PlaySound("AtkB", 2); PlaySound("Die", 12); } }
		}

		//Jump
		else if(CurrAnm.IsName("Dilo|IdleJumpStart") | CurrAnm.IsName("Dilo|RunJumpStart") | CurrAnm.IsName("Dilo|JumpIdle") |
			CurrAnm.IsName("Dilo|IdleJumpEnd") | CurrAnm.IsName("Dilo|RunJumpEnd") | CurrAnm.IsName("Dilo|JumpAtk"))
		{
			shared.IsJumping=true;
			transform.rotation*= Quaternion.Euler(0, anm.GetFloat("Turn")*MAXANG, 0);
			if(CurrAnm.IsName("Dilo|IdleJumpStart") | CurrAnm.IsName("Dilo|RunJumpStart"))
			{
				PlaySound("Step", 1); PlaySound("Step", 2);
				if(CurrAnm.normalizedTime > 0.4) body.AddForce(Vector3.up*260*transform.localScale.x); 
				if(CurrAnm.IsName("Dilo|RunJumpStart")) body.AddForce(dir*160*transform.localScale.x*anm.speed);
			}
			else if(CurrAnm.IsName("Dilo|IdleJumpEnd") | CurrAnm.IsName("Dilo|RunJumpEnd"))
			{ 
				body.drag=4; body.velocity = new Vector3(body.velocity.x, 0, body.velocity.z);
				if(CurrAnm.IsName("Dilo|RunJumpEnd")) body.AddForce(transform.forward*160*transform.localScale.x*anm.speed);
				PlaySound("Step", 3); PlaySound("Step", 4); 
			}
			else { body.drag=0.1f; if(CurrAnm.IsName("Dilo|JumpAtk")) { shared.IsAttacking=true; PlaySound("AtkB", 1); PlaySound("Bite", 9); } }
		}

		//Forward
		else if(NextAnm.IsName("Dilo|Walk") | CurrAnm.IsName("Dilo|Walk") | CurrAnm.IsName("Dilo|WalkGrowl"))
		{
			transform.rotation*= Quaternion.Euler(0, anm.GetFloat("Turn")*MAXANG, 0);
			body.AddForce(transform.forward*32*transform.localScale.x*anm.speed);
			if(CurrAnm.IsName("Dilo|Walk")){ PlaySound("Step", 6); PlaySound("Step", 14);}
			else if(CurrAnm.IsName("Dilo|WalkGrowl")) { PlaySound("AtkA", 2); PlaySound("Step", 6); PlaySound("Step", 14); }
		}

		//Running
		else if(NextAnm.IsName("Dilo|Run") | CurrAnm.IsName("Dilo|Run") |
		   CurrAnm.IsName("Dilo|RunGrowl") | CurrAnm.IsName("Dilo|RunAtk1") |
		   (CurrAnm.IsName("Dilo|RunAtk2") && CurrAnm.normalizedTime < 0.9) |
		   (CurrAnm.IsName("Dilo|IdleAtk3") && CurrAnm.normalizedTime > 0.5 && CurrAnm.normalizedTime < 0.9))
		{
			transform.rotation*= Quaternion.Euler(0, anm.GetFloat("Turn")*MAXANG, 0);
			body.AddForce(transform.forward*160*transform.localScale.x*anm.speed);
			if(CurrAnm.IsName("Dilo|Run")){ PlaySound("Step", 4); PlaySound("Step", 12); }
			else if(CurrAnm.IsName("Dilo|RunGrowl")) { PlaySound("AtkB", 2); PlaySound("Step", 4); PlaySound("Step", 12); }
			else if( CurrAnm.IsName("Dilo|RunAtk1")) { shared.IsAttacking=true; PlaySound("AtkB", 2); PlaySound("Step", 4); PlaySound("Step", 12); }
			else if( CurrAnm.IsName("Dilo|RunAtk2")| CurrAnm.IsName("Dilo|IdleAtk3"))
			{ shared.IsAttacking=true; PlaySound("Growl", 2); PlaySound("Step", 4); PlaySound("Bite", 9); PlaySound("Step", 12); }
		}
		
		//Backward
		else if(NextAnm.IsName("Dilo|Walk-") | NextAnm.IsName("Dilo|WalkGrowl-") |
					CurrAnm.IsName("Dilo|Walk-") | CurrAnm.IsName("Dilo|WalkGrowl-"))
		{
			if(CurrAnm.normalizedTime > 0.25 && CurrAnm.normalizedTime < 0.45| 
			 CurrAnm.normalizedTime > 0.75 && CurrAnm.normalizedTime < 0.9)
			{
				body.AddForce(transform.forward*-32*transform.localScale.x*anm.speed);
				transform.rotation*= Quaternion.Euler(0, anm.GetFloat("Turn")*MAXANG, 0);
			}
			if(CurrAnm.IsName("Dilo|WalkGrowl-")) { PlaySound("Growl", 1); PlaySound("Step", 6); PlaySound("Step", 13); }
			else { PlaySound("Step", 6); PlaySound("Step", 13); }
		}

		//Strafe/Turn right
		else if(NextAnm.IsName("Dilo|Strafe-") | CurrAnm.IsName("Dilo|Strafe-"))
		{
			transform.rotation*= Quaternion.Euler(0, anm.GetFloat("Turn")*MAXANG, 0);
			body.AddForce(transform.right*16*transform.localScale.x*anm.speed);
			PlaySound("Step", 6); PlaySound("Step", 14);
		}
		
		//Strafe/Turn left
		else if(NextAnm.IsName("Dilo|Strafe+") | CurrAnm.IsName("Dilo|Strafe+"))
		{
			transform.rotation*= Quaternion.Euler(0, anm.GetFloat("Turn")*MAXANG, 0);
			body.AddForce(transform.right*-16*transform.localScale.x*anm.speed);
			PlaySound("Step", 6); PlaySound("Step", 14);
		}

		//Various
		else if(CurrAnm.IsName("Dilo|IdleAtk3")) { shared.IsAttacking=true; transform.rotation*= Quaternion.Euler(0, anm.GetFloat("Turn")*MAXANG, 0); PlaySound("Growl", 1); }
		else if(CurrAnm.IsName("Dilo|GroundAtk")) { shared.IsAttacking=true; PlaySound("AtkB", 2); PlaySound("Bite", 4); }
		else if(CurrAnm.IsName("Dilo|IdleAtk1") | CurrAnm.IsName("Dilo|IdleAtk2"))
		{ shared.IsAttacking=true; transform.rotation*= Quaternion.Euler(0, anm.GetFloat("Turn")*MAXANG, 0); PlaySound("AtkA", 2); PlaySound("Bite", 9); }
		else if(CurrAnm.IsName("Dilo|ToSleep")) { reset=true; shared.IsConstrained=true; }
		else if(CurrAnm.IsName("Dilo|Sleep")) { reset=true; PlaySound("Sleep", 1); shared.IsConstrained=true;}
		else if(CurrAnm.IsName("Dilo|EatA")) { reset=true; PlaySound("Food", 1); }
		else if(CurrAnm.IsName("Dilo|EatB")) { reset=true; PlaySound("Bite", 3); }
		else if(CurrAnm.IsName("Dilo|EatC")) reset=true;
		else if(CurrAnm.IsName("Dilo|IdleC")) PlaySound("Growl", 1);
		else if(CurrAnm.IsName("Dilo|IdleD")) PlaySound("Growl", 1);
		else if(CurrAnm.IsName("Dilo|IdleE")) { reset=true; PlaySound("Bite", 4); PlaySound("Bite", 7); PlaySound("Bite", 9); }
		else if(CurrAnm.IsName("Dilo|Die-")) { reset=true; PlaySound("AtkA", 1);  shared.IsDead=false; }
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
			shared.lastHit--; Head.GetChild(0).transform.rotation*= Quaternion.Euler(shared.lastHit, 0, 0);
		}
		else if(reset) //Reset
		{
			anm.SetFloat("Turn", Mathf.Lerp(anm.GetFloat("Turn"), 0.0f, ANGT/3));
			spineX=Mathf.Lerp(spineX, 0.0f, ANGT/3);
			spineY=Mathf.Lerp(spineY, 0.0f, ANGT/3);
			crouch=Mathf.Lerp(crouch, 0, ANGT/3);
		}
		else
		{
			shared.TargetLooking(spineX, spineY,crouch);
			spineX=Mathf.Lerp(spineX, shared.spineX_T, ANGT/3);
			spineY=Mathf.Lerp(spineY, shared.spineY_T, ANGT/3);
			crouch=Mathf.Lerp(crouch, shared.crouch_T, ANGT);
		}
		
		//Save head position befor transformation
		shared.fixedHeadPos=Head.position;

		//Spine rotation
		float spineZ =spineY*spineX/MAXYAW;
		Spine0.transform.rotation*= Quaternion.Euler(-spineY, spineZ, -spineX);
		Spine1.transform.rotation*= Quaternion.Euler(-spineY, spineZ, -spineX);
		Spine2.transform.rotation*= Quaternion.Euler(-spineY, spineZ, -spineX);
		Spine3.transform.rotation*= Quaternion.Euler(-spineY, spineZ, -spineX);
		Spine4.transform.rotation*= Quaternion.Euler(-spineY, spineZ, -spineX);
		Spine5.transform.rotation*= Quaternion.Euler(-spineY, spineZ, -spineX);
		
		Neck0.transform.rotation*= Quaternion.Euler(-spineY, spineZ, -spineX);
		Neck1.transform.rotation*= Quaternion.Euler(-spineY, spineZ, -spineX);
		Neck2.transform.rotation*= Quaternion.Euler(-spineY, spineZ, -spineX);
		Neck3.transform.rotation*= Quaternion.Euler(-spineY, spineZ, -spineX);
		Head.transform.rotation*= Quaternion.Euler(-spineY, spineZ, -spineX);

		//Tail rotation
		tailX=Mathf.Lerp(tailX, anm.GetFloat("Turn")*MAXYAW, ANGT/3);
		Tail0.transform.rotation*= Quaternion.Euler(0, 0, tailX);
		Tail1.transform.rotation*= Quaternion.Euler(0, 0, tailX);
		Tail2.transform.rotation*= Quaternion.Euler(0, 0, tailX);
		Tail3.transform.rotation*= Quaternion.Euler(0, 0, tailX);
		Tail4.transform.rotation*= Quaternion.Euler(0, 0, tailX);
		Tail5.transform.rotation*= Quaternion.Euler(0, 0, tailX);
		Tail6.transform.rotation*= Quaternion.Euler(0, 0, tailX);
		Tail7.transform.rotation*= Quaternion.Euler(0, 0, tailX);
		Tail8.transform.rotation*= Quaternion.Euler(0, 0, tailX);
		Tail9.transform.rotation*= Quaternion.Euler(0, 0, tailX);
		Tail10.transform.rotation*= Quaternion.Euler(0, 0, tailX);
		Tail11.transform.rotation*= Quaternion.Euler(0, 0, tailX);
		
		//Arms rotation
		Arm1.transform.rotation*= Quaternion.Euler(spineY*8, 0, 0);
		Arm2.transform.rotation*= Quaternion.Euler(0, spineY*8, 0);

		//IK feet (require "JP script extension" asset)
		shared.SmallBipedIK(Right_Hips, Right_Leg, Right_Foot0, Right_Foot1, Left_Hips, Left_Leg, Left_Foot0, Left_Foot1);
		//Check for ground layer
		shared.GetGroundAlt(false, crouch);

		//*************************************************************************************************************************************************
		// CPU (require "JP script extension" asset)
		if(shared.AI && shared.Health!=0) { shared.AICore(1, 2, 3, 4, 5, 6, 7); }
		//*************************************************************************************************************************************************
		// Human
		else if(shared.Health!=0) { shared.GetUserInputs(1, 2, 3, 4, 5, 6, 7); }
		//*************************************************************************************************************************************************
		//Dead
		else { anm.SetBool("Attack", false); anm.SetInteger ("Move", 0); anm.SetInteger ("Idle", -1); }
	}

}