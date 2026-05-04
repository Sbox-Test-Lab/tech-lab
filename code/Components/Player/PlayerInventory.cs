using System;
using System.Text.Json;
using System.Text.Json.Nodes;



public partial class PlayerInventory : ItemContainer
{
	public int CurrentItemIndex { get; set; } = 0;

	/// <summary>
	/// The currently equipped item GameObject, if any.
	/// </summary>
	public GameObject EquippedItem { get; private set; }

	public void UnequipItem()
	{
		if ( EquippedItem.IsValid() )
		{
			EquippedItem.Destroy();
			EquippedItem = null;
		}

		// Clear hold pose on the body
		var player = Player.LocalPlayer();
		if ( player is not null )
		{
			var holdPose = player.Body.Components.GetOrCreate<PlayerHoldPose>();
			holdPose.SetConfig( null );
		}
	}

	public void EquipItem()
	{
		UnequipItem();

		var player = Player.LocalPlayer();
		var go = ItemFactory.CreateFromState( GetItemState( CurrentItemIndex ), Vector3.Zero );
		var item = go.Components.Get<Item>();

		// Disable physics so the item doesn't fall or collide
		if ( go.Components.TryGet<Rigidbody>( out var rb ) )
		{
			rb.Enabled = false;
		}

		foreach ( var col in go.Components.GetAll<Collider>() )
		{
			col.Enabled = false;
		}

		// Attach to the player's right hand bone
		var renderer = player.Body.Components.Get<SkinnedModelRenderer>();
		var hand = renderer.GetBoneObject( "hold_R" );

		go.SetParent( hand );
		go.LocalPosition = Vector3.Zero;
		go.LocalRotation = Rotation.Identity;
		go.LocalScale = Vector3.One;

		// Apply HoldTypeResource offsets and activate PlayerHoldPose
		//var equipable = go.Components.Get<Equipable>( FindMode.EverythingInSelfAndDescendants );
		var holdConfig = item.Resource.HoldType;

		if ( holdConfig is not null )
		{
			go.LocalPosition = holdConfig.PositionOffset;
			go.LocalRotation = Rotation.From( holdConfig.RotationOffset );
			go.LocalScale = holdConfig.Scale == Vector3.Zero ? Vector3.One : holdConfig.Scale;
		}

		var holdPose = player.Body.Components.GetOrCreate<PlayerHoldPose>();
		holdPose.SetConfig( holdConfig );

		EquippedItem = go;
	}

    protected override void OnUpdate()
    {
        base.OnUpdate();

        // Handle Scroll Wheel Input  
        var wheel = Input.MouseWheel;

        if (Input.Pressed("NextSlot")) wheel.y = -1;
        if (Input.Pressed("PrevSlot")) wheel.y = 1;

        if (wheel.y == 0f) return;

        // Get the Next Available Equipment Item  
        CurrentItemIndex += (int)wheel.y;

        // Ensure the index wraps around within bounds  
        if (Items.Count == 0) return; // Prevent index out of range when Items is empty  

        if (CurrentItemIndex < 0)
            CurrentItemIndex = Items.Count - 1;
        else if (CurrentItemIndex >= Items.Count)
            CurrentItemIndex = 0;

        // Assign Item to Current Slot  
        var selectedItem = Items.ElementAt(CurrentItemIndex);

		bool isEquipable = HasBehavior<Equipable>(CurrentItemIndex);
		Log.Info( $"{isEquipable}" );
		// Switch to Equipment Item  
		if ( isEquipable )
		{
			EquipItem();
		}
	}

}
