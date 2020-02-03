using UnityEngine;

public class SplineWalker4 : MonoBehaviour
{

    //public BezierSpline spline;
    public BezierSplineMulti multi;

    public float duration;

    public bool lookForward;

    public SplineWalkerMode mode;

    private float progress;
    private bool goingForward = true;
    private bool jump = false;

    private void Update()
    {
        if (goingForward)
        {
            progress += Time.deltaTime / duration;
            if (progress > 1f)
            {
                if (mode == SplineWalkerMode.Once)
                {
                    progress = 1f;
                }
                else if (mode == SplineWalkerMode.Loop)
                {
                    progress -= 1f;
                }
                else
                {
                    progress = 2f - progress;
                    goingForward = false;
                }
            }
        }
        else
        {
            progress -= Time.deltaTime / duration;
            if (progress < 0f)
            {
                progress = -progress;
                goingForward = true;
            }
        }


        Vector3 position = multi.splines[1].GetPoint(progress);
        if (jump)
            position = multi.splines[0].GetPoint(progress);
        Vector3 cPoint = multi.splines[0].GetControlPoint(0);
        float d = Vector3.Distance(cPoint, position);
        if (d < 0.05f && !jump) 
        {
            Debug.Log("Position x: " + position.x + ", y: " + position.y + ", distance: " + d);
            position = cPoint;
            progress = 0f;
            jump = true;
        }
        /*if (progress > 0.5f) {
            Debug.Log ("Progress: " + progress + ", at point: " + position);
        }*/
        position.Set(position.x, position.y, position.z - .02f);
        transform.localPosition = position;
        if (lookForward)
        {
            //transform.LookAt(position + spline.GetDirection(progress));
            if (!jump)
                transform.LookAt(position + multi.splines[1].GetDirection(progress), Vector3.forward);
            else
                transform.LookAt(position + multi.splines[0].GetDirection(progress), Vector3.forward);
            transform.Rotate(90f, -90f, 0);
        }
    }
}
