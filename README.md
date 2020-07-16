# UnityTeleportKit2D
The teleports create seamless wrapping by making a copy of the object that enters, projecting it onto the other portal and setting it as a child of what entered.
Because of this, collisions with the object's copy should work normally but limitations follow.

Parent = Object that enters the portal

Child = Copy of that object set as a child to create seamless wrapping

  Advantages: 
  -Seamless wrapping
  -handles collision within seamless wrapping

  Limitations:
  -Objects can't rotate whilst seamless wrapping ***(because the object's copy is a child of the object and thus rotates relative to its parent)***
  -Linked portals need to have matching rotation ***(because the child moves matching with its parent)***
  -Seamless wrapping is only visible for spriteRenderers ***(This is simply because I only copy the parent's Collider2D and SpriteRenderer)***
