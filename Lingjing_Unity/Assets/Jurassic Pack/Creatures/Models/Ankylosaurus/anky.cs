using UnityEngine;

public class anky : MonoBehaviour
{
	public AudioClip Waterflush, Hit_jaw, Hit_head, Hit_tail, Medstep, Medsplash, Idleherb, Sniff1, Chew, Largestep, Largesplash, Anky1, Anky2, Anky3, Anky4;
	Transform Spine0, Spine1, Spine2, Neck0, Neck1, Neck2, Neck3, Head, Tail0, Tail1, Tail2, Tail3, Tail4, Tail5, 
	Left_Arm0, Right_Arm0, Left_Arm1, Right_Arm1, Left_Hand, Right_Hand, 
	Left_Hips, Right_Hips, Left_Leg, Right_Leg, Left_Foot, Right_Foot;
	float crouch, spineX, spineY, tailX; bool reset;
	const float MAXYAW=25, MAXPITCH=10, MAXCROUCH=4, MAXANG=1.5f, ANGT=0.025f;

	Vector3 dir;
	shared shared;
	AudioSource[] source;
	Animator anm;
	Rigidbody body;

	//*************************************************************************************************************************************************
	//Get components
	void Start()
	{
		Left_Hips = transform.Find ("Anky/root/pelvis/left hips");
		Right_Hips = transform.Find ("Anky/root/pelvis/right hips");
		Left_Leg  = transform.Find ("Anky/root/pelvis/left hips/left leg");
		Right_Leg = transform.Find ("Anky/root/pelvis/right hips/right leg");
		
		Left_Foot = transform.Find ("Anky/root/pelvis/left hips/left leg/left foot");
		Right_Foot = transform.Find ("Anky/root/pelvis/right hips/right leg/right foot");
		
		Left_Arm0 = transform.Find ("Anky/root/spine0/spine1/spine2/spine3/spine4/left arm0");
		Right_Arm0 = transform.Find ("Anky/root/spine0/spine1/spine2/spine3/spine4/right arm0");
		Left_Arm1 = transform.Find ("Anky/root/spine0/spine1/spine2/spine3/spine4/left arm0/left arm1");
		Right_Arm1 = transform.Find ("Anky/root/spine0/spine1/spine2/spine3/spine4/right arm0/right arm1");
		
		Left_Hand = transform.Find ("Anky/root/spine0/spine1/spine2/spine3/spine4/left arm0/left arm1/left hand");
		Right_Hand = transform.Find ("Anky/root/spine0/spine1/spine2/spine3/spine4/right arm0/right arm1/right hand");
	
		Tail0 = transform.Find ("Anky/root/pelvis/tail0");
		Tail1 = transform.Find ("Anky/root/pelvis/tail0/tail1");
		Tail2 = transform.Find ("Anky/root/pelvis/tail0/tail1/tail2");
		Tail3 = transform.Find ("Anky/root/pelvis/tail0/tail1/tail2/tail3");
		Tail4 = transform.Find ("Anky/root/pelvis/tail0/tail1/tail2/tail3/tail4");
		Tail5 = transform.Find ("Anky/root/pelvis/tail0/tail1/tail2/tail3/tail4/tail5");

		Neck0 = transform.Find ("Anky/root/spine0/spine1/spine2/spine3/spine4/neck0");
		Neck1 = transform.Find ("Anky/root/spine0/spine1/spine2/spine3/spine4/neck0/neck1");
		Neck2 = transform.Find ("Anky/root/spine0/spine1/spine2/spine3/spine4/neck0/neck1/neck2");
		Neck3 = transform.Find ("Anky/root/spine0/spine1/spine2/spine3/spine4/neck0/neck1/neck2/neck3");
		Head = transform.Find ("Anky/root/spine0/spine1/spine2/spine3/spine4/neck0/neck1/neck2/neck3/head");
		
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
		switch (rndPainsnd) { case 0: painSnd=Anky1; break; case 1: painSnd=Anky2; break; case 2: painSnd=Anky3; break; case 3: painSnd=Anky4; break; }
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
				else if(shared.IsOnWater) source[1].PlayOneShot(Medsplash, Random.Range(0.25f, 0.5f));
				else if(shared.IsOnGround) source[1].PlayOneShot(Medstep, Random.Range(0.25f, 0.5f));
				shared.lastframe=shared.currframe; break;
			case "Hit": source[1].pitch=Random.Range(1.0f, 1.25f); source[1].PlayOneShot(shared.IsOnWater|shared.IsInWater?Largesplash:Largestep, 1.0f);
							shared.lastframe=shared.currframe; break;
			case "Die": source[0].pitch=Random.Range(1.0f, 1.25f); source[1].PlayOneShot(shared.IsOnWater|shared.IsInWater?Largesplash:Largestep, 1.0f);
							shared.lastframe=shared.currframe; shared.IsDead=true; break;
			case "Sniff": source[0].pitch=Random.Range(1.0f, 1.25f); source[0].PlayOneShot(Sniff1, 0.5f);
							shared.lastframe=shared.currframe; break;
			case "Chew": source[0].pitch=Random.Range(1.0f, 1.25f); source[0].PlayOneShot(Chew, 0.5f);
							shared.lastframe=shared.currframe; break;
			case "Sleep": source[0].pitch=Random.Range(1.0f, 1.25f); source[0].PlayOneShot(Idleherb, 0.25f);
							shared.lastframe=shared.currframe; break;
			case "Growl": int rnd = Random.Range (0, 4); source[0].pitch=Random.Range(1.0f, 1.25f);
						 if(rnd==0)source[0].PlayOneShot(Anky1, 1.0f);
						 else if(rnd==1)source[0].PlayOneShot(Anky2, 1.0f);
						 else if(rnd==2)source[0].PlayOneShot(Anky3, 1.0f);
						 else if(rnd==3)source[0].PlayOneShot(Anky4, 1.0f);
						 shared.lastframe=shared.currframe; break;
			}
		}
	}

	//*************************************************************************************************************************************************
	// Add forces to the Rigidbody
	void FixedUpdate()
	{
		if(!shared.IsActive) { body.Sleep(); return; }
		reset=false; shared.IsAttacking=false; shared.HasSideAttack=false; shared.IsConstrained= false;
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
		if(NextAnm.IsName("Anky|Idle1A") | NextAnm.IsName("Anky|Idle2A") | CurrAnm.IsName("Anky|Idle1A") | CurrAnm.IsName("Anky|Idle2A") |
			CurrAnm.IsName("Anky|Die1") | CurrAnm.IsName("Anky|Die2"))
		{
			if(CurrAnm.IsName("Anky|Die1")) { reset=true; shared.IsConstrained=true; if(!shared.IsDead) { PlaySound("Growl", 2); PlaySound("Die", 12); } }
			else if(CurrAnm.IsName("Anky|Die2")) { reset=true; shared.IsConstrained=true; if(!shared.IsDead) { PlaySound("Growl", 2); PlaySound("Die", 10); } }
		}

		//Forward
		if(CurrAnm.IsName("Anky|Walk") | CurrAnm.IsName("Anky|WalkGrowl") | NextAnm.IsName("Anky|Step1") | CurrAnm.IsName("Anky|Step1") |
			NextAnm.IsName("Anky|Step2") | CurrAnm.IsName("Anky|Step2") | CurrAnm.IsName("Anky|ToIdle2C") | CurrAnm.IsName("Anky|ToEatA") |
			(CurrAnm.IsName("Anky|ToEatC") && CurrAnm.normalizedTime < 0.9))
		{
			transform.rotation*= Quaternion.Euler(0, anm.GetFloat("Turn")*MAXANG, 0);
			body.AddForce(transform.forward*15*transform.localScale.x*anm.speed);
			if(CurrAnm.IsName("Anky|WalkGrowl")) { PlaySound("Growl", 1); PlaySound("Step", 6); PlaySound("Step", 13); }
			else if(CurrAnm.IsName("Anky|Walk")) { PlaySound("Step", 6); PlaySound("Step", 13); }
			else PlaySound("Step", 9);
		}

		//Running
		else if(NextAnm.IsName("Anky|Run") | CurrAnm.IsName("Anky|Run") | CurrAnm.IsName("Anky|RunGrowl"))
		{
			transform.rotation*= Quaternion.Euler(0, anm.GetFloat("Turn")*MAXANG, 0);
			body.AddForce(transform.forward*60*transform.localScale.x*anm.speed);
			if(CurrAnm.IsName("Anky|Run") | NextAnm.IsName("Anky|Run")) { PlaySound("Step", 3); PlaySound("Step", 9); }
			else { PlaySound("Growl", 2); PlaySound("Step", 3); PlaySound("Step", 9); }
		}
		
		//Backward
		else if(CurrAnm.IsName("Anky|Step1-") | CurrAnm.IsName("Anky|Step2-") | CurrAnm.IsName("Anky|ToIdle1C") | CurrAnm.IsName("Anky|ToSit1"))
		{
			transform.rotation*= Quaternion.Euler(0, -anm.GetFloat("Turn")*MAXANG, 0);
			body.AddForce(transform.forward*-15*transform.localScale.x*anm.speed);
			PlaySound("Step", 9);
		}

		//Strafe/Turn right
		else if(NextAnm.IsName("Anky|Strafe1-") | CurrAnm.IsName("Anky|Strafe1-") | NextAnm.IsName("Anky|Strafe2+") | CurrAnm.IsName("Anky|Strafe2+"))
		{
			transform.rotation*= Quaternion.Euler(0, anm.GetFloat("Turn")*MAXANG, 0);
			body.AddForce(transform.right*8*transform.localScale.x*anm.speed);
			PlaySound("Step", 5); PlaySound("Step", 12);
		}

		//Strafe/Turn left
		else if(NextAnm.IsName("Anky|Strafe1+") | CurrAnm.IsName("Anky|Strafe1+") | NextAnm.IsName("Anky|Strafe2-") | CurrAnm.IsName("Anky|Strafe2-"))
		{
			transform.rotation*= Quaternion.Euler(0, anm.GetFloat("Turn")*MAXANG, 0);
			body.AddForce(transform.right*-8*transform.localScale.x*anm.speed);
			PlaySound("Step", 5); PlaySound("Step", 12);
		}

		//Attack 
		else if(CurrAnm.IsName("Anky|Atk1-") | CurrAnm.IsName("Anky|Atk1+") | CurrAnm.IsName("Anky|Atk2-") | CurrAnm.IsName("Anky|Atk2+"))
		{
			shared.HasSideAttack=true; shared.IsAttacking=true;
			if(CurrAnm.normalizedTime < 0.9)
			{
				if(CurrAnm.IsName("Anky|Atk1-")) transform.rotation*= Quaternion.Euler(0, Mathf.Lerp(0, -10.0f, 0.5f), 0);
				else if(CurrAnm.IsName("Anky|Atk2+")) transform.rotation*= Quaternion.Euler(0, Mathf.Lerp(0, -7.0f, 0.5f), 0);
				else if(CurrAnm.IsName("Anky|Atk2-")) transform.rotation*= Quaternion.Euler(0, Mathf.Lerp(0, 7.0f, 0.5f), 0);
				else if(CurrAnm.IsName("Anky|Atk1+")) transform.rotation*= Quaternion.Euler(0, Mathf.Lerp(0, 10.0f, 0.5f), 0);
			}
			PlaySound("Hit", 8); PlaySound("Hit", 10); 
			if(CurrAnm.IsName("Anky|Atk1-") | CurrAnm.IsName("Anky|Atk1+")) { PlaySound("Sniff", 3); PlaySound("Growl", 2); }
		}
		
		//Various
		if(CurrAnm.IsName("Anky|EatA")) PlaySound("Chew", 10);
		else if(CurrAnm.IsName("Anky|EatB")) { PlaySound("Chew", 1); PlaySound("Chew", 4); PlaySound("Chew", 8); PlaySound("Chew", 12); }
		else if(CurrAnm.IsName("Anky|EatC")) reset=true;
		else if(CurrAnm.IsName("Anky|ToSit")) shared.IsConstrained=true; 
		else if(CurrAnm.IsName("Anky|SitIdle")) shared.IsConstrained=true; 
		else if(CurrAnm.IsName("Anky|Sleep")) { reset=true; shared.IsConstrained=true; PlaySound("Sleep", 2); }
		else if(CurrAnm.IsName("Anky|SitGrowl")) { shared.IsConstrained=true; PlaySound("Growl", 2); PlaySound("Step", 8); }
		else if(CurrAnm.IsName("Anky|AtkIdle")) shared.HasSideAttack=true;
		else if(CurrAnm.IsName("Anky|AtkGrowl")) { shared.IsConstrained=true; shared.IsAttacking=true; shared.HasSideAttack=true; PlaySound("Growl", 2); PlaySound("Hit", 7); }
		else if(CurrAnm.IsName("Anky|Atk0")) { shared.IsAttacking=true; shared.HasSideAttack=true; PlaySound("Growl", 2); PlaySound("Sniff", 3); }
		else if(CurrAnm.IsName("Anky|Idle1B")) PlaySound("Growl", 2); 
		else if(CurrAnm.IsName("Anky|Idle1C")) PlaySound("Growl", 2);
		else if(CurrAnm.IsName("Anky|Idle2B")) PlaySound("Growl", 2);
		else if(CurrAnm.IsName("Anky|Idle2C")) { reset=true; PlaySound("Sniff", 1); }
		else if(CurrAnm.IsName("Anky|Die1-")) { PlaySound("Growl", 3);  shared.IsDead=false; }
		else if(CurrAnm.IsName("Anky|Die2-")) { PlaySound("Growl", 3);  shared.IsDead=false; }
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
		Neck0.transform.rotation*= Quaternion.Euler(spineY, -spineZ, -spineX);
		Neck1.transform.rotation*= Quaternion.Euler(spineY, -spineZ, -spineX);
		Neck2.transform.rotation*= Quaternion.Euler(spineY, -spineZ, -spineX);
		Neck3.transform.rotation*= Quaternion.Euler(spineY, -spineZ, -spineX);
		Head.transform.rotation*= Quaternion.Euler(spineY, -spineZ, -spineX);

		//Tail rotation
		tailX=Mathf.Lerp(tailX, anm.GetFloat("Turn")*MAXYAW, ANGT);
		Tail0.transform.rotation*= Quaternion.Euler(0, 0, tailX);
		Tail1.transform.rotation*= Quaternion.Euler(0, 0, tailX);
		Tail2.transform.rotation*= Quaternion.Euler(0, 0, tailX);
		Tail3.transform.rotation*= Quaternion.Euler(0, 0, tailX);
		Tail4.transform.rotation*= Quaternion.Euler(0, 0, tailX);
		Tail5.transform.rotation*= Quaternion.Euler(0, 0, tailX);

		//IK feet (require "JP script extension" asset)
		//shared.QuadIK( Right_Arm0, Right_Arm1, Right_Hand, Left_Arm0, Left_Arm1, Left_Hand, Right_Hips, Right_Leg, Right_Foot, Left_Hips, Left_Leg, Left_Foot, -0.4f*transform.localScale.x);
		//Check for ground layer
		//shared.GetGroundAlt(true, crouch);

		//*************************************************************************************************************************************************
		// CPU (require "JP script extension" asset)
		//if(shared.AI && shared.Health!=0) { shared.AICore(1, 2, 3, 0, 4, 5, 6); }
		//*************************************************************************************************************************************************
		// Human
		//else if(shared.Health!=0) { shared.GetUserInputs(1, 2, 3, 0, 4, 5, 6); }
		//*************************************************************************************************************************************************
		//Dead
		//else { anm.SetBool("Attack", false); anm.SetInteger ("Move", 0); anm.SetInteger ("Idle", -1); }
	}
}




