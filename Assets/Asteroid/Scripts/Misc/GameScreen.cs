using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScreen
{
    private static Rect gameScreen;
    private const float screenBorderOffset = 0.025f;
    private static Camera camera;
    public static float Width { get { return gameScreen.width; } }

    public static void Init(Camera cam)
    {
        camera = cam;

        float cameraOffset = cam.transform.position.y;
        Vector3 upperRight, lowerLeft;

        //create rect representing game field based on camera with offsets for smooth object transition
        lowerLeft = cam.ViewportToWorldPoint(new Vector3(-screenBorderOffset, -screenBorderOffset, cameraOffset));
        upperRight = cam.ViewportToWorldPoint(new Vector3(1+ screenBorderOffset, 1+ screenBorderOffset, cameraOffset));
        gameScreen = new Rect(lowerLeft.x, lowerLeft.z, Mathf.Abs(lowerLeft.x - upperRight.x), Mathf.Abs(lowerLeft.z - upperRight.z));
    }

    public static bool Contains(Vector3 position)
    {
        return gameScreen.Contains(new Vector2(position.x, position.z));
    }

    public static Vector3 GetRandomPointForUFO(out Vector3 moveDirection)
    {
        //20% offset
        float verticalOffset = gameScreen.height * 0.2f;

        int rand = Random.Range(0, 2);

        if (rand == 0)
        {
            //spawn on left corner
            moveDirection = Vector3.right;
            return new Vector3(gameScreen.x, 0, Random.Range(gameScreen.y + verticalOffset,gameScreen.y + gameScreen.height - verticalOffset));
        }
        else
        {
            //spawn on right corner
            moveDirection = Vector3.left;
            return new Vector3(gameScreen.x + gameScreen.width, 0, Random.Range(gameScreen.y + verticalOffset, gameScreen.y + gameScreen.height - verticalOffset));
        }
    }
    public static Vector3 GetRandomPointOnScreenEdge()
    {
        int rand = Random.Range(0, 4);

        if(rand == 0) //left area
            return new Vector3(gameScreen.x, 0, Random.Range(gameScreen.y, gameScreen.y + gameScreen.height));
        else if (rand == 1) //right area
            return new Vector3(gameScreen.x + gameScreen.width, 0, Random.Range(gameScreen.y, gameScreen.y + gameScreen.height));
        else if (rand == 2) //top area
            return new Vector3(Random.Range(gameScreen.x, gameScreen.x + gameScreen.width), 0, gameScreen.y + gameScreen.height);
        else //bottom area
            return new Vector3(Random.Range(gameScreen.x, gameScreen.x + gameScreen.width), 0, gameScreen.y);
        
    }

    public static Vector3 GetMousePositionInWorldSpace()
    {
        Vector3 mouse = Input.mousePosition;
        mouse.z = camera.transform.position.y;
        return camera.ScreenToWorldPoint(mouse);
    }
    public static Vector3 GetRandomPointOnScreen()
    {
        float verticalOffset = gameScreen.height * 0.3f;
        float horizontalOffset = gameScreen.width * 0.3f;

        return new Vector3(
            Random.Range(gameScreen.x + horizontalOffset,gameScreen.x + gameScreen.width - horizontalOffset),
            0,
            Random.Range(gameScreen.y + verticalOffset, gameScreen.y + gameScreen.height - verticalOffset));
    }

    public static Vector3 GetScreenClampedPosition(Vector3 position)
    {
        if (gameScreen.Contains(new Vector2(position.x, position.z)))
            return position;

        if (position.x > gameScreen.x + gameScreen.width)
            position.x -= gameScreen.width;
        else if (position.x < gameScreen.x)
            position.x += gameScreen.width;

        if (position.z > gameScreen.y + gameScreen.height)
            position.z -= gameScreen.height;
        else if (position.z < gameScreen.y)
            position.z += gameScreen.height;

        return position;

    }
}
