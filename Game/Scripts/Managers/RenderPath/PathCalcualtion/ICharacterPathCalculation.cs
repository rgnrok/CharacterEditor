 using System.Collections.Generic;
 using CharacterEditor;
 using UnityEngine;

 public interface ICharacterPathCalculation : IService
 {
     void SetCharacter(Character character);
     bool CalculatePath(Vector3 to, out Vector3[] points);

     float PathDistance(Vector3 endPoint);
 }