using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public Material waveMaterial;
    public Texture2D waveTexture;
    public bool reflectiveBoundary;

    float[][] waveN, waveNm1, waveNp1; // state info

    float Lx = 10; // wave width
    float Ly = 10; // wave height
    [SerializeField] float dx = 0.01f; // x density
    float dy { get => dx; } // y density
    int nx, ny; // resolution

    public float CFL = 0.5f;
    public float c = 1;
    float dt, t; // time step (delta time) & current time
    [SerializeField] float floatToColorMultiplier = 2f; // to emphasize the color
    [SerializeField] float pulseFrequency = 0.2f; // control wave pulse speed
    [SerializeField] float pulseMagnitude = 1f;
    [SerializeField] Vector2Int pulsePosition = new Vector2Int(50, 50);
    [SerializeField] float elasticity = 0.98f; // how long before the wave disipates
    public int slitY, slitX1, slitX2;
    
    // Start is called before the first frame update
    void Start()
    {
        nx = Mathf.FloorToInt(Lx / dx);
        ny = Mathf.FloorToInt(Ly / dy);
        print(nx);
        print(ny);
        waveTexture = new Texture2D(nx, ny, TextureFormat.RGBA32, false);

        waveN = new float[nx][];
        waveNm1 = new float[nx][];
        waveNp1 = new float[nx][];

        for (int i=0; i<nx; i++) {
            waveN[i] = new float[ny];
            waveNp1[i] = new float[ny];
            waveNm1[i] = new float[ny];
        }

        waveMaterial.SetTexture("_MainTex", waveTexture); // coloring
        waveMaterial.SetTexture("_Displacement", waveTexture); // displacement
    }

    void WaveStep() {
        dt = CFL * dx / c; // recalculate dx
        t += dx; // increment time

        if (reflectiveBoundary) {
            ApplyReflectiveBoundary();
        } else {
            ApplyAbsorptiveBoundary();
        }

        for (int i=0; i<nx; i++) {
            for (int j=0; j<ny; j++) {
                waveNm1[i][j] = waveN[i][j]; // copy to prev (minus 1)
                waveN[i][j] = waveNp1[i][j]; // copy from after (plus 1)
            }
        }

        // dripping effect
        waveN[pulsePosition.x][pulsePosition.y] = dt * dt * 20 * pulseMagnitude * Mathf.Sin(t * Mathf.Rad2Deg * pulseFrequency);

        for(int i=1; i<nx - 1; i++) { // do not process edges
            for (int j=1; j<ny-1; j++) {
                if (j == slitY) {
                    if (!(i == slitX1 || i == slitX2)) continue;
                }
                float n_ij = waveN[i][j];
                float n_ip1j = waveN[i+1][j];
                float n_im1j = waveN[i-1][j];
                float n_ijp1 = waveN[i][j+1];
                float n_ijm1 = waveN[i][j-1];
                float nm1_ij = waveNm1[i][j];
                waveNp1[i][j] = 2f * n_ij - nm1_ij + CFL * CFL * (n_ijm1 + n_ijp1 + n_im1j + n_ip1j - 4f * n_ij); // wave equation
                waveNp1[i][j] *= elasticity;
            }
        }
    }

    void ApplyMatrixToTexture(float[][] state, ref Texture2D tex) {
        for(int i=0; i<nx; i++) {
            for(int j=0; j<ny; j++) {
                float val = state[i][j] * floatToColorMultiplier;
                tex.SetPixel(i,j, new Color(val + 0.5f, val + 0.5f, val + 0.5f, 1f)); // grayscale point color
            }
        }
        tex.Apply();
    }

    void ApplyReflectiveBoundary() {
        for (int i=0; i<nx; i++) {
            waveN[i][0] = 0f;
            waveN[i][ny-1] = 0f;
        }

        for (int j = 0; j < ny; j++) {
            waveN[0][j] = 0f;
            waveN[nx-1][j] = 0f;
        }

        for (int i=0;i<nx;i++) {
            // y, x
            if ((i == slitX1 || i == slitX2)) continue;
            waveN[i][slitY] = 0f;
        }

        float v = (CFL - 1f) / (CFL + 1f);

        for(int i=0; i<nx; i++) {
            waveNp1[i][0] = waveN[i][1] + v * (waveNp1[i][1] - waveN[i][0]);
        }
    }

    void ApplyAbsorptiveBoundary() {
        float v = (CFL - 1f) / (CFL + 1f);

        for(int i=0; i<nx; i++) {
            waveNp1[i][0] = waveN[i][1] + v * (waveNp1[i][1] - waveN[i][0]);
            waveNp1[i][ny - 1] = waveN[i][ny - 2] + v * (waveNp1[i][ny - 2] - waveN[i][ny - 1]);
        }

        for (int j = 0; j < ny; j++) {
            waveNp1[0][j] = waveN[1][j] + v * (waveNp1[1][j] - waveN[0][j]);
            waveNp1[nx - 1][j] = waveN[nx - 2][j] + v * (waveNp1[nx-2][j]-waveN[nx-1][j]);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            print(contact.thisCollider.name + " hit " + contact.normal);
            // Visualize the contact point
            Debug.DrawRay(contact.point, contact.normal, Color.white);
        }
    }

    void MousePositionOnTexture(ref Vector2Int pos) {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit)) {
            pos = new Vector2Int((int)(hit.textureCoord.x * nx), (int)(hit.textureCoord.y * ny));
        }
    }

    // Update is called once per frame
    void Update() {
        MousePositionOnTexture(ref pulsePosition);
        WaveStep();
        ApplyMatrixToTexture(waveN, ref waveTexture);
    }
}
