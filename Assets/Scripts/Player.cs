using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public CatmullRomPath path;
    public Text speedText;
    public Text lapText;
    public Text WinnerText;
    public SlotCar Rival;


    public float initialSpeed = 0;
    public float maxSpeed = 36.0f;
    public float speedIncrement = 1.5f;
    public float closeEnough = 0.1f;

    public float upcomingAngleClamp = 90.0f;
    public int numSegmentsToLookAhead = 8;
    private float inputAxis;

    private float speed;
    private int nextPoint;
    private int previousPoint;
    private int Lap;

    [SerializeField]
    int lapmax;
    private bool winner;  // Variable privada para almacenar el estado

    public bool Winner
    {
        get { return winner; }  // Accede a la variable privada
        set { winner = value; } // Modifica la variable privada
    }

    // Start is called before the first frame update
    void Start()
    {
        // Set the initial speed
        speed = initialSpeed;

        // Ensure the path has points
        if (path.Points.Count == 0)
            path.RebuildPath();

        // Set the next point to 1
        nextPoint = 1;
        previousPoint = 0;

        // Get the first path point's location and set the transform's position
        Vector3 location = path.Points[0];
        transform.position = location;

        // Get the second path point's location and then calculate the direction
        Vector3 nextLocation = path.Points[1];
        Vector3 direction = nextLocation - location;

        // Calculate the look rotation and then set the transform's rotation
        transform.rotation = Quaternion.LookRotation(direction);
        WinnerText.gameObject.SetActive(false);
        Winner = false;

    }

    // Update is called once per frame
    void Update()
    {
        if (inputAxis != 0)
        {
            if(inputAxis == 1)
            {
                speed += speedIncrement *inputAxis* Time.deltaTime;
            }
            else if (inputAxis == -1 && speed > 0)
            {
                speed -= speedIncrement * inputAxis * Time.deltaTime;
                //if (speed < 0) speed = 0;
            }               
            speed = Mathf.Clamp(speed, -1, maxSpeed);
        }
        else
        {
            if (speed > 0)
            {
                speed -= speedIncrement * Time.deltaTime;
                //if (speed < 0) speed = 0;
            }
        }

        if (speed > 0)
        {
        // Get the current location and the next location
        Vector3 location = transform.position;
        Vector3 nextLocation = path.Points[nextPoint];

        // Calculate the direction vector and then get the forward direction vector
        Vector3 direction = (nextLocation - location).normalized;
        Vector3 forward = transform.forward;

        // Validate that the next point is in front of the car
        ValidateNextPoint(forward, ref direction, ref nextLocation, ref location);

        // Then rotate the car to match the rotation of the point they are traveling towards
        gameObject.transform.rotation = Quaternion.LookRotation(direction);

        //Look ahead by several points and determine the upcoming angle and adjust speed if necessary
        int index = CalculatePointIndexAfterIncrement(nextPoint, numSegmentsToLookAhead);
        float upcomingAngle = Vector3.Angle(transform.forward, (path.Points[index] - transform.position).normalized);
        float speedPCT = (1.0f - (Mathf.Clamp(upcomingAngle, 0.0f, upcomingAngleClamp) / upcomingAngleClamp));

        speedPCT = speedPCT * speedPCT; // Square the speed PCT to enhance the effect

        // Translate the card, factoring in the speed and delta time
        gameObject.transform.Translate(Vector3.forward * speed * speedPCT * Time.deltaTime);

        // Check if the car has arrived at the next path point location 
        CheckNextPathPoint(gameObject.transform.position, path.Points[nextPoint]);

            // Set the debug text
            speedText.text = "[" + nextPoint + "] - Speed: " + (speed * speedPCT).ToString("0.00") + " / " + maxSpeed;

            //speedText.text =  "Input: "+ inputAxis;
            lapText.text = "[" + Lap + "] / 10";
        WinnerText.text = "RED WINS!";
        }
        else
        {

        }
    }

    private void ValidateNextPoint(Vector3 forward, ref Vector3 direction, ref Vector3 nextLocation, ref Vector3 location)
    {
        // Get the angle 
        float angle = Vector3.Angle(forward, direction);

        // If the angle is almost 180, that means the next point is behind up.
        // Calculate the next point instead and validate it
        if (angle >= 178.0f)
        {
            nextPoint = CalculatePointIndexAfterIncrement(nextPoint);

            nextLocation = path.Points[nextPoint];
            direction = (nextLocation - location).normalized;

            ValidateNextPoint(forward, ref direction, ref nextLocation, ref location);
        }
    }

    private void CheckNextPathPoint(Vector3 currentPosition, Vector3 nextPathPoint)
    {
        // Check the distance from the current position to the next path point location
        if (Vector3.Distance(currentPosition, nextPathPoint) < closeEnough)
        {
            previousPoint = nextPoint;
            nextPoint = CalculatePointIndexAfterIncrement(nextPoint);

            // We've completed a full lap, increase the speed
            if (nextPoint < previousPoint)
            {
                if (Lap < 5)
                {

                    Lap++;
                }
                else
                {
                    if (!Rival.Winner)
                    {
                        Winner = true;
                        WinnerText.gameObject.SetActive(true);
                    }

                }


            }

            // Check the next path point to ensure that we haven't passed it as well
            CheckNextPathPoint(currentPosition, path.Points[nextPoint]);
        }
    }

    private int CalculatePointIndexAfterIncrement(int currentIndex, int increment = 1)
    {
        int index = (currentIndex + increment) % path.Points.Count;
        return index;
    }


    private void IncreaseSpeed()
    {
        speed += speedIncrement;
        if (speed > maxSpeed)
        {
            speed = maxSpeed;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        inputAxis = context.ReadValue<float>();
        Debug.Log("Input Axis: " + inputAxis);
        Debug.Log("Input Speed: " + speed);
    }

    public void OnReturn(InputAction.CallbackContext context)
    {
        if (context.performed) // Si se presiona Espacio
        {
            transform.position = path.Points[previousPoint]; // Regresa al punto anterior
            Vector3 direction = (path.Points[nextPoint] - path.Points[previousPoint]).normalized;
            transform.rotation = Quaternion.LookRotation(direction); // Ajusta la rotación
            speed = 0;
        }
    }
}
