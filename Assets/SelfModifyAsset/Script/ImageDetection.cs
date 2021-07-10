using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCvSharp;
using System.Threading.Tasks;

public class ImageDetection : MonoBehaviour
{
    WebCamTexture _webCamTexture;
    public RawImage webcamInput;
    public RawImage leftHandInputThresh;
    public RawImage rightHandInputThresh;
    public int threshValue;

    public bool actualGame = true;

    private bool leftFingerZeroTrigger = false;
    private bool leftFingerTwoTrigger = false;
    private bool leftFingerThreeTrigger = false;
    private bool leftFingerFiveTrigger = false;

    private bool rightFingerZeroTrigger = false;
    private bool rightFingerTwoTrigger = false;
    private bool rightFingerThreeTrigger = false;
    private bool rightFingerFiveTrigger = false;

    public bool shieldGesture = false;
    public bool fireGesture = false;
    public bool moveLeftGesture = false;
    public bool moveRightGesture = false;

    public int leftFingerCount = 0;
    public int rightFingerCount = 0;

    void Start()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        _webCamTexture = new WebCamTexture(devices[0].name,800,500);
        _webCamTexture.Play();

    }

    void Update()
    {
        threshValue = GameObject.FindGameObjectWithTag("GameDefaultSetupManager").GetComponent<GameDefaultSetupManager>().threshValue;
        Mat frame = OpenCvSharp.Unity.TextureToMat(_webCamTexture);
        int frameWidth = frame.Width;
        int frameHeight = frame.Height;

        //ROI (yStart,yEnd,xStart,xEnd) 
        Mat LeftROI = frame[frame.Height / 2, frame.Height, 0, frame.Width * 3 / 10];
        detectHandGesture(LeftROI, 1);
        setupDetectionArea(LeftROI);

        Mat RightROI = frame[frame.Height / 2, frame.Height, frame.Width * 7 / 10, frame.Width];
        detectHandGesture(RightROI, 2);
        setupDetectionArea(RightROI);

        Texture newtexture = OpenCvSharp.Unity.MatToTexture(frame);
        webcamInput.texture = newtexture;

        if (actualGame)
            determineInteraction();

    }

    void setupDetectionArea(Mat frame)
    {
        OpenCvSharp.Rect detectionArea = new OpenCvSharp.Rect(0, 0, frame.Width, frame.Height);
        frame.Rectangle(detectionArea, new Scalar(0, 250, 0), 2);
          
    }

    //left hand = 1, right hand = 2
    void detectHandGesture(Mat ROI, int hand)
    {
        int ROIarea = ROI.Width * ROI.Height;

        //Gray scale image     
        Mat grayMat = new Mat();
        Cv2.CvtColor(ROI, grayMat, ColorConversionCodes.BGR2GRAY);
        //GaussianBlur image
        Mat blur = new Mat();
        Cv2.GaussianBlur(grayMat, blur, new Size(9, 9), 0);
        //Set threshold for image
        Mat thresh = new Mat();
        Cv2.Threshold(blur, thresh, threshValue, 255, ThresholdTypes.Binary);

        if (hand == 1) {
            Texture newtexture2 = OpenCvSharp.Unity.MatToTexture(thresh);
            rightHandInputThresh.texture = newtexture2;
        }
        else if(hand == 2)
        {
            Texture newtexture2 = OpenCvSharp.Unity.MatToTexture(thresh);
            leftHandInputThresh.texture = newtexture2;
        }

        //Hand Recognition
        Point[][] contours;
        Point[] hull;
        Vec4i[] defectsX;
        Mat hullMatrix = new Mat();
        Mat defects = new Mat();
        HierarchyIndex[] hierarchy;
        int defectsCount = 0;
        
        Cv2.FindContours(thresh, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple, null);
        for (int i = 0; i < contours.Length; i++)
        {
            if (Cv2.ContourArea(InputArray.Create(contours[i])) > (ROIarea / 10))
            {
                //Find center
                Moments m = Cv2.Moments(contours[i]);
                int cx = (int)(m.M10 / m.M00);
                int cy = (int)(m.M01 / m.M00);

                //Find Convex Hull and All Convexity Defect
                hull = Cv2.ConvexHull(contours[i], false);
                Cv2.ConvexHull(InputArray.Create(contours[i]), hullMatrix, false, false);
                Cv2.ConvexityDefects(InputArray.Create(contours[i]), hullMatrix, defects);

                //Determine the defect of the finger
                if (defects.Rows != 0 && defects.Cols != 0)
                {
                    defectsX = new Vec4i[defects.Rows];
                    for (int j = 0; j < defects.Rows; j++)
                    {
                        defectsX[j] = defects.At<Vec4i>(j);
                        Point start = contours[i][defectsX[j].Item0];
                        Point end = contours[i][defectsX[j].Item1];
                        Point far = contours[i][defectsX[j].Item2];

                        //Calculation of the degree between start, far and end
                        //Calculate all three distance value for the triangle
                        float distanceSE = Mathf.Sqrt(Mathf.Pow(start.X - end.X, 2) + Mathf.Pow(start.Y - end.Y, 2));
                        float distanceSF = Mathf.Sqrt(Mathf.Pow(start.X - far.X, 2) + Mathf.Pow(start.Y - far.Y, 2));
                        float distanceEF = Mathf.Sqrt(Mathf.Pow(end.X - far.X, 2) + Mathf.Pow(end.Y - far.Y, 2));

                        float angleAtFar = Mathf.Acos((Mathf.Pow(distanceSF, 2) + Mathf.Pow(distanceEF, 2) - Mathf.Pow(distanceSE, 2)) / (2 * distanceSF * distanceEF)) * 180 / (float)3.142;

                        if (angleAtFar < 100)
                        {
                            Cv2.Circle(ROI, far, 10, new Scalar(0, 255, 0));
                            defectsCount++;

                        }

                    }
                }

                //Left hand
                if(hand == 1)
                {
                    if (defectsCount > 1)
                    {
                        if (defectsCount > 4)
                            leftFingerCount = 5;
                        else
                            leftFingerCount = defectsCount + 1;
                    }
                    else
                        leftFingerCount = 0;

                    switch (defectsCount)
                    {
                        case 0: { leftFingerZeroTrigger = true; break; }
                        case 1: { leftFingerTwoTrigger = true; break; }
                        case 2: { leftFingerThreeTrigger = true; break; }
                        case 4: { leftFingerFiveTrigger = true; break; }
                        default: break;

                    }
                }
                //Right Hand
                else
                {
                    if (defectsCount > 1)
                    {
                        if (defectsCount > 4)
                            rightFingerCount = 5;
                        else
                          rightFingerCount = defectsCount + 1;
                    }
                    else
                        rightFingerCount = 0;

                    switch (defectsCount)
                    {
                        case 0: { rightFingerZeroTrigger = true; break; }
                        case 1: { rightFingerTwoTrigger = true; break; }
                        case 2: { rightFingerThreeTrigger = true; break; }
                        case 4: { rightFingerFiveTrigger = true; break; }
                        default: break;

                    }
                }

                Cv2.DrawContours(ROI, new Point[][] { contours[i] }, 0, new Scalar(250, 0, 0), 2);
                Cv2.DrawContours(ROI, new Point[][] { hull }, 0, new Scalar(0, 0, 250), 2);
            }
        }

    }

    public void resetAllFingerTrigger()
    {
        leftFingerZeroTrigger = false;
        leftFingerTwoTrigger = false;
        leftFingerThreeTrigger = false;
        leftFingerFiveTrigger = false;

        rightFingerZeroTrigger = false;
        rightFingerTwoTrigger = false;
        rightFingerThreeTrigger = false;
        rightFingerFiveTrigger = false;

        shieldGesture = false;
        fireGesture = false;
        moveLeftGesture = false;
        moveRightGesture = false;

    }

    //Shield - Both Hand 5
    //Fire - One 5 one 5 to 0
    void determineInteraction()
    {
        if (leftFingerFiveTrigger && rightFingerFiveTrigger)
        {
            shieldGesture = true;
        }

        if (leftFingerThreeTrigger && rightFingerFiveTrigger)
        {
            if (rightFingerZeroTrigger && !shieldGesture)
            {
                fireGesture = true;
            }

        }

        if (leftFingerFiveTrigger && rightFingerThreeTrigger)
        {
            if (leftFingerZeroTrigger && !shieldGesture)
            {
                fireGesture = true;
            }

        }

    }

    public void stopUsingCamera()
    {
        _webCamTexture.Stop();
    }

    public void startUsingCamera()
    {
        _webCamTexture.Play();
    }

}
