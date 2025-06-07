using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.Serialization;

public class CameraScript : MonoBehaviour
{

   public float mDelta = 10; // Pixels. The width border at the edge in which the movement work;
   public Camera cam;

   [Serializable]
   public class Movable
   {
      public Transform movableGroup;
   } 
   public Vector3 movablesUp = new Vector3(0, 140, 0); 
   public Vector3 movablesDown = new Vector3(0, -140, 0); 
   public Vector3 movablesStart = new Vector3(0, 0, 0);
   public Movable[] movables;
    
   void Update ()
   {  
   if (Input.mousePosition.y >= Screen.height - mDelta)
   {
      cam.transform.DOMoveY(1f, 1f);
      foreach (Movable movable in movables)
      {
         movable.movableGroup.DOLocalMove(movablesDown, 1f);
         
      }
   }

   if (Input.mousePosition.y <= 0 + mDelta)
   {
      cam.transform.DOMoveY(-1f, 1f);
      foreach (Movable movable in movables)
      {
         movable.movableGroup.DOLocalMove(movablesUp, 1f);
      }
   }

   if (Input.mousePosition.y <= Screen.height - mDelta && Input.mousePosition.y > 0 + mDelta)
   {
      cam.transform.DOMoveY(0f, 1f);
      foreach (Movable movable in movables)
      {
         movable.movableGroup.DOLocalMove(movablesStart, 1f);
      }
   }
   }

}
