using UnityEngine;
using System.Collections;

public class Pacdot : MonoBehaviour {

	void OnTriggerEnter2D(Collider2D other)
	{
		if(other.name == "pacman")
		{
			GameManager.score += 10;
		    GameObject[] pacdots = GameObject.FindGameObjectsWithTag("pacdot");
            Destroy(gameObject);

            PlayerController.dotCount++;
            PlayerController.cycleTracker = 0;


            if (pacdots.Length == 1)
		    {
		        GameObject.FindObjectOfType<GameGUINavigation>().LoadLevel();
		    }
		}
	}
}
