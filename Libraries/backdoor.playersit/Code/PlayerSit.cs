using Sandbox;
using Sandbox.Citizen;
using System.Collections;
using System.Collections.Generic;

[Title( "Sit Controller" ), Icon( "chair" )]
public sealed class PlayerSit : Component, PlayerController.IEvents
{
    [Property] private PlayerController Controller {get;set;}
    [Property] private CitizenAnimationHelper Animator {get;set;}

    private float DuckHeight;
    private RealTimeSince Cooldown;
    private bool SetupChairAngle = false;
    private Vector3 SittingPosition;
    private Dictionary<string, List<Vector3>> ApprovedSeats;

    [Change("UpdatePlayer")] private Prop Chair {get;set;}
    private void UpdatePlayer(){
        if (Chair is null){
            Controller.UseAnimatorControls = true; // force sitting animation
            Controller.Body.MotionEnabled = true; // prevent clientside jittering
            Controller.EnableFootstepSounds = true;
            Controller.DuckedHeight = DuckHeight;
            Animator.Sitting = CitizenAnimationHelper.SittingStyle.None;
        }
        else{
            Controller.UseAnimatorControls = false; // force sitting animation
            Controller.Body.MotionEnabled = false; // prevent clientside jittering
            Controller.EnableFootstepSounds = false;
            Controller.DuckedHeight = DuckHeight * 2;
            Animator.Sitting = CitizenAnimationHelper.SittingStyle.Chair;
        }
    }

    protected override void OnStart(){
        DuckHeight = Controller.DuckedHeight;
        
        if (Animator.Target.Model.Name == "models/citizen/citizen.vmdl"){
            ApprovedSeats = new Dictionary<string, List<Vector3>>{
                {"models/citizen_props/chair01.vmdl", new List<Vector3>{ new Vector3(6, 0, 3) }},
                {"models/citizen_props/chair02.vmdl", new List<Vector3>{ new Vector3(8, 0, 2) }},
                {"models/citizen_props/chair03.vmdl", new List<Vector3>{ new Vector3(9, 0, 2) }},
                {"models/citizen_props/chair04blackleather.vmdl", new List<Vector3>{ new Vector3(9, 30, 2), new Vector3(9, -30, 2) }},
                {"models/citizen_props/chair05bluefabric.vmdl", new List<Vector3>{ new Vector3(12, 25, 2), new Vector3(12, -25, 2) }}
            };
        }
        else{
            ApprovedSeats = new Dictionary<string, List<Vector3>>{
                {"models/citizen_props/chair01.vmdl", new List<Vector3>{ new Vector3(6, 0, 0) }},
                {"models/citizen_props/chair02.vmdl", new List<Vector3>{ new Vector3(10, 0, 0) }},
                {"models/citizen_props/chair03.vmdl", new List<Vector3>{ new Vector3(10, 0, 0) }},
                {"models/citizen_props/chair04blackleather.vmdl", new List<Vector3>{ new Vector3(10, 30, 0), new Vector3(10, -30, 0) }},
                {"models/citizen_props/chair05bluefabric.vmdl", new List<Vector3>{ new Vector3(14, 25, 0), new Vector3(14, -25, 0) }}
            };
        }
    }

    protected override void OnUpdate(){
        Chair = this.GameObject.Parent.Components.Get<Prop>();

        if (!IsProxy && Chair is not null && Input.Pressed("Jump") && Cooldown > 1){
            Cooldown = 0;
            Controller.GameObject.SetParent(null);
            return;
        }
        if (Chair is null) return;

        Controller.LocalPosition = SittingPosition;
        Controller.Body.Velocity = 0; // prevent clientside jittering
        Controller.IsDucking = false; // prevent model ducking

        Animator.IsGrounded = true; // prevent falling animation
        Animator.GameObject.WorldRotation = Chair.WorldRotation;
        Animator.WithLook(Controller.EyeAngles.Forward);
    }

	void PlayerController.IEvents.PostCameraSetup(CameraComponent camera){
        Prop chair = this.GameObject.Parent.Components.Get<Prop>();
        if (chair is not null && !Controller.ThirdPerson){
            camera.WorldPosition = Controller.EyePosition + new Vector3(0, 0, -15);
        }
        if (SetupChairAngle){
            Controller.EyeAngles = chair.WorldRotation.Angles();
            SetupChairAngle = false;
        }
	}

	Component PlayerController.IEvents.GetUsableComponent(GameObject GameObject){
        var Prop = GameObject.GetComponent<Prop>();
        if (Prop is not null && ApprovedSeats.ContainsKey(Prop.Model.Name)){
            return Prop;
        }
		return default;
	}

	void PlayerController.IEvents.StartPressing(Component component){
        Prop chair = component.GetComponent<Prop>();

        // Remove spots that a player is already sitting in
        List<Vector3> UnoccupiedSeats = new List<Vector3>(ApprovedSeats[chair.Model.Name]);
        foreach (var p in chair.GameObject.Children){
            if (ApprovedSeats[chair.Model.Name].Contains(p.LocalPosition)){
                UnoccupiedSeats.Remove(p.LocalPosition);
            }
        }
        if (UnoccupiedSeats.Count == 0) return;

        var trace = Game.SceneTrace.Ray(Scene.Camera.Transform.World.ForwardRay, Controller.ReachLength + (Controller.ThirdPerson ? Controller.CameraOffset.x : 0)).WithoutTags("player");
        SceneTraceResult tr = trace.Run();
        if (!tr.Hit) return;

        // Look for closest available spot nearest to where the player is looking
        Vector3 closest = UnoccupiedSeats[0];
        float min = 0f;
        foreach (var p in UnoccupiedSeats){
            float dist = tr.HitPosition.DistanceSquared((p + chair.GameObject.WorldPosition).RotateAround(chair.GameObject.WorldPosition, chair.GameObject.WorldRotation)); // rotate the ApprovedSeats position and get the distance to trace.hitpos

            if (dist < min || min is 0){
                closest = p;
                min = dist;
            }
        }

        SittingPosition = closest;
        if (Controller.GameObject.Parent.Components.Get<Prop>() is null){ // don't change player facing angle if already sitting
            SetupChairAngle = true;
        }
        Controller.GameObject.SetParent(chair.GameObject, true);
	}
}
