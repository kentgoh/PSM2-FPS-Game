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
    public RawImage HandInputThresh;
    public int threshValue;
    public int fingerCount = 0;

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
        Mat ROI = frame[frame.Height * 1 / 5, frame.Height * 4 / 5, frame.Width / 2, frame.Width];
        detectHandGesture(ROI, 1);
        setupDetectionArea(ROI);

        Texture newtexture = OpenCvSharp.Unity.MatToTexture(frame);
        webcamInput.texture = newtexture;

    }

    void setupDetectionArea(Mat frame)
    {
        OpenCvSharp.Rect detectionArea = new OpenCvSharp.Rect(0, 0, frame.Width, frame.Height);
        frame.Rectangle(detectionArea, new Scalar(0, 0, 250), 2);
          
    }

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

        Texture newtexture2 = OpenCvSharp.Unity.MatToTexture(thresh);
        HandInputThresh.texture = newtexture2;

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
                            Cv2.Circle(ROI, far, 10, new Scalar(0, 0, 255));
                            defectsCount++;

                        }

                    }
                }
                if (defectsCount > 0 && defectsCount < 5)
                    fingerCount = defectsCount + 1;
                else
                    fingerCount = 0;

                Cv2.DrawContours(ROI, new Point[][] { contours[i] }, 0, new Scalar(250, 0, 0), 2);
                Cv2.DrawContours(ROI, new Point[][] { hull }, 0, new Scalar(0, 250, 0), 2);
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
