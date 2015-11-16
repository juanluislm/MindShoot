var speed = 6.0;
var jumpSpeed = 8.0;
var gravity = 20.0;

private var moveDirection = Vector3.zero;
private var grounded : boolean = false;

private var gotNunchuckData : boolean = false;
private var fromNunchuckX : float;
private var fromNunchuckY : float;
private var fromNunchuckJump : boolean;

function FixedUpdate() {
	if (grounded) {
		// We are grounded, so recalculate movedirection directly from axes
		if (gotNunchuckData) {
			moveDirection = new Vector3(fromNunchuckX, 0, fromNunchuckY);
			gotNunchuckData = false;
		} else moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
		moveDirection = transform.TransformDirection(moveDirection);
		moveDirection *= speed;
		
		if (Input.GetButton ("Jump") || fromNunchuckJump ) {
			moveDirection.y = jumpSpeed;
			fromNunchuckJump = false;
		}
	}

	// Apply gravity
	moveDirection.y -= gravity * Time.deltaTime;
	
	// Move the controller
	var controller : CharacterController = GetComponent(CharacterController);
	var flags = controller.Move(moveDirection * Time.deltaTime);
	grounded = (flags & CollisionFlags.CollidedBelow) != 0;
}

public function Jump() {
	fromNunchuckJump = true;
}

public function nunchuckData( x, y )
{
	fromNunchuckX = (x-128) / 128.0;
	fromNunchuckY = (y-128) / 128.0;
	deadenInput();
	gotNunchuckData = true;
}


public function nunchuckDataX( x )
{
	fromNunchuckX = (x - 128) / 128.0;
	deadenInput();
	gotNunchuckData = true;
}

public function nunchuckDataY( y )
{
	fromNunchuckY = (y-128) / 128.0;
	deadenInput();
	gotNunchuckData = true;
}

function deadenInput() {
	var deadZoneBegin = -.15;
	var deadZoneEnd = .15;
	
	if( fromNunchuckX >= deadZoneBegin && fromNunchuckX <= deadZoneEnd ) 
	   fromNunchuckX = 0;
	else
	{
	   if( fromNunchuckX > 0 )
	      fromNunchuckX = ( fromNunchuckX - deadZoneBegin )  / ( 1.0 - deadZoneBegin );
	   else
	      fromNunchuckX = ( fromNunchuckX + deadZoneBegin ) / ( 1.0 - deadZoneBegin );
	}
	if( fromNunchuckY >= deadZoneBegin && fromNunchuckY <= deadZoneEnd ) 
	   fromNunchuckY = 0;
	else
	{
	   if( fromNunchuckY > 0 )
	      fromNunchuckY = ( fromNunchuckY - deadZoneBegin )  / ( 1.0 - deadZoneBegin );
	   else
	      fromNunchuckY = ( fromNunchuckY + deadZoneBegin ) / ( 1.0 - deadZoneBegin );
	}
}


@script RequireComponent(CharacterController)