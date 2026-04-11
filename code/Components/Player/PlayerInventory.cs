using System;
using System.Text.Json;
using System.Text.Json.Nodes;



public partial class PlayerInventory : ItemContainer
{
	public int CurrentItemIndex { get; set; } = 0;

	
	public void EquipItem()
	{
	

 
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
	
		// Switch to Equipment Item  
		//if( HasEquipableComponent(CurrentItemIndex) )
		//{
			//EquipItem();
		//}
	}

}
