# Items

Each item lives in its own folder named after the item. All files related to that item live together.

## Structure

```
items/
  <item-name>/
    <item-name>.item        ← ItemResource asset (name, description, icon, prefab reference)
    <item-name>.prefab      ← Prefab with Item component + all behavior components
    <item-name>.holdtype    ← HoldTypeResource (grip offset, animation, finger curl)
    <item-name>_icon.png    ← Inventory thumbnail
```

## Example

```
items/
  sword/
    sword.item
    sword.prefab
    sword.holdtype
    sword_icon.png
  pistol/
    pistol.item
    pistol.prefab
    pistol.holdtype
    pistol_icon.png
```

## Notes

- Not every item needs a `.holdtype` — only items that are held by the player.
- The `.item` file references the `.prefab` and `_icon.png` by path.
- Shared/reusable hold types not tied to a specific item go in `Assets/shared/holdtypes/`.
