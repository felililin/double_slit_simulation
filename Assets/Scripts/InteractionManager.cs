using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    private WaveManager waveManager;
    private GameObject wall1, wall2, wall3;
    // Start is called before the first frame update
    void Start()
    {
        waveManager = gameObject.GetComponent<WaveManager>();
        wall1 = gameObject.transform.Find("Wall1").gameObject; // right most
        wall2 = gameObject.transform.Find("Wall2").gameObject; // center
        wall3 = gameObject.transform.Find("Wall3").gameObject; // left most
        UpdateWallPosition();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.W)) {
            waveManager.slitY -= 1;
            waveManager.ResetWave();
            UpdateWallPosition();
        }
        if (Input.GetKeyUp(KeyCode.A)) {
            waveManager.slitX1 += 1;
            waveManager.slitX2 += 1;
            waveManager.ResetWave();
            UpdateWallPosition();
        }
        if (Input.GetKeyUp(KeyCode.S)) {
            waveManager.slitY += 1;
            waveManager.ResetWave();
            UpdateWallPosition();
        }
        if (Input.GetKeyUp(KeyCode.D)) {
            waveManager.slitX1 -= 1;
            waveManager.slitX2 -= 1;
            waveManager.ResetWave();
            UpdateWallPosition();
        }

        if (Input.GetKeyUp(KeyCode.Plus) || Input.GetKeyUp(KeyCode.Equals)) {
            waveManager.slitWidth += 1;
            waveManager.ResetWave();
            UpdateWallPosition();
        }
        if ((Input.GetKeyUp(KeyCode.Minus) || Input.GetKeyUp(KeyCode.Underscore)) && waveManager.slitWidth > 1) {
            waveManager.slitWidth -= 1;
            waveManager.ResetWave();
            UpdateWallPosition();
        }

        if (Input.GetKeyUp(KeyCode.Q)) {
            if (waveManager.slitX1 < waveManager.slitX2 && waveManager.slitX1 < 100) {
                waveManager.slitX1 += 1;
                waveManager.slitX2 -= 1;
                waveManager.ResetWave();
                UpdateWallPosition();
            }
        }
        if (Input.GetKeyUp(KeyCode.E)) {
            waveManager.slitX1 -= 1;
            waveManager.slitX2 += 1;
            waveManager.ResetWave();
            UpdateWallPosition();
        }
    }

    void UpdateWallPosition() {
        // x, -, y
        // total 200 points, 100 itu pas titik tengah
        float y = (100 - waveManager.slitY) * waveManager.dx;
        float width1 = (waveManager.slitX1 - (waveManager.slitWidth / 2)) * waveManager.dx;
        float wall1X = (100 - waveManager.slitX1 + waveManager.slitWidth / 2f) * waveManager.dx + (width1 / 2);
        wall1.transform.position = new Vector3(wall1X, 0, y);
        wall1.transform.localScale = new Vector3(width1, 1, 0.1f);

        float width2 = ((waveManager.slitX2 - waveManager.slitX1)) * waveManager.dx;
        // 110, 90 -> diff: 20
        // 108, 88 -> diff: 20
        // 109, 91 -> diff: 18
        // 107, 89 -> diff: 18 -> center: 7+11 / 2
        float wall2X = ((waveManager.slitX2 + waveManager.slitX1) / 2 - 100) * waveManager.dx;
        // wall2X = 0;
        Debug.Log(wall2X);
        float wall2XOffset = ((100 - waveManager.slitX1 - waveManager.slitWidth)/2) * waveManager.dx;
        // Debug.Log(wall2XOffset);
        wall2.transform.position = new Vector3(-wall2X, 0, y);
        wall2.transform.localScale = new Vector3(width2, 1, 0.1f);

        float width3 = (200 - waveManager.slitX2 - (waveManager.slitWidth / 2)) * waveManager.dx;
        float wall3X = (waveManager.slitX2 - 100 + waveManager.slitWidth) * waveManager.dx + (width3 / 2);
        wall3.transform.position = new Vector3(-wall3X, 0, y);
        wall3.transform.localScale = new Vector3(width3, 1, 0.1f);
    }
}
