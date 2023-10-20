using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TimeController : MonoBehaviour {

    public Transform sun;
    public Transform dot1;
    public Transform dot2;
    public Transform dot3;
    public Transform dot4;
    public Image mask;
    public Image overlay;

    private float time;
    private float time_speed;
    private float view_radius;
    private float dying_fadeout;

    // References.
    [HideInInspector]
    // Player is assigned by the player after he is spawned.
    public Player player;
    GameController gameController;
    ItemController items;

    [SerializeField]
    Material circleMaskPrefab;
    Material maskMaterialInstance;
    private static readonly int Radius = Shader.PropertyToID("_Radius");

    void Awake() {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
        items = GameObject.Find("ItemController").GetComponent<ItemController>();
    }

    void Start () {
        time = 0;
        time_speed = 1f / 45;

        maskMaterialInstance = Instantiate(circleMaskPrefab);
        maskMaterialInstance.renderQueue = mask.defaultMaterial.renderQueue;
        mask.material = maskMaterialInstance;
        
        //mask.rectTransform.
    }

	void Update () {
        if (player == null) {
            return;
        }

        if (player.energy <= 0) {
            player.energy = 0;
            time_speed = Utils.f_approach(time_speed, 1, 0.02f);
        }
        else {
            if (gameController.difficulty == 1) {
                time_speed = Utils.f_approach(time_speed, 1f / 30, 0.05f);
            }
            else {
                time_speed = Utils.f_approach(time_speed, 1f / 45, 0.05f);
            }
        }

        time += time_speed;
        if (time > 360) {
            time = 0;
            player.food--;

            if (player.food <= 0) {
                player.dying = true;
                SoundController.instance.PlaySound("snd_dying");
                player.GetComponent<SpriteAnimator>().Play("s_player_dying");
                player.GetComponent<SpriteAnimator>().currentFrame = 0;
                player.GetComponent<SpriteAnimator>().fps = 5;
            }
            else {
                player.energy = player.energy_max;
                f_grow();
            }
        }

        // In GameMaker the sin function takes an angle in degrees
        // while in Unity it expects an angle in radians.
        view_radius = 240 + 120 * Mathf.Sin(time * Mathf.Deg2Rad);

        // Map the radius value which goes between 120 to 360 to 0.33 to 1
        // and pass it to the mask shader.
        float maskFloat = view_radius / 360;
        // maskMaterialInstance.SetFloat(Radius, maskFloat);
        // maskMaterialInstance.renderQueue = mask.defaultMaterial.renderQueue;
        // mask.material = maskMaterialInstance;
        mask.material.SetFloat(Radius, maskFloat);
        
        DrawCircle();

        Color tempColor = overlay.color;
        if (time >= 45 && time <= 135) {
            tempColor.a = 0;
        }
        else if (time <= 315 && time >= 225) {
            tempColor.a = 0.7f;
        }
        else if (time < 45 || time > 315) {
            tempColor.a = 0.35f - 0.35f * Mathf.Sin(time * 2 * Mathf.Deg2Rad);
        }
        else {
            tempColor.a = 0.35f + 0.35f * Mathf.Sin(time * 2 * Mathf.Deg2Rad);
        }

        if (player.dying) {
            if (dying_fadeout > 0 || time >= 135) {
                dying_fadeout++;
                tempColor.a = Mathf.Clamp(dying_fadeout / 120, 0, 1);
                if (dying_fadeout >= 360) {
                    SceneManager.LoadScene(0);
                }
                if (time > 270) {
                    time = 270;
                }
            }
        }

        overlay.color = tempColor;
    }

    void DrawCircle() {
        var _r = view_radius / 2;
        var _a = 180 + time;
        var _x = 0;
        var _y = 0;

        var camPos = Camera.main.transform.position;
        dot1.position = new Vector3(
            camPos.x + Mathf.Round(_x + _r + 0.5f),
            camPos.y + Mathf.Round(_y),
            0
        );
        dot2.position = new Vector3(
            camPos.x + Mathf.Round(_x - _r + 0.5f),
            camPos.y + Mathf.Round(_y),
            0
        );
        dot3.position = new Vector3(
            camPos.x + Mathf.Round(_x),
            camPos.y + Mathf.Round(_y - _r + 0.5f),
            0
        );
        dot4.position = new Vector3(
            camPos.x + Mathf.Round(_x),
            camPos.y + Mathf.Round(_y + _r + 0.5f),
            0
        );

        // In GameMaker the sin and cos function takes an angle in degrees while in Unity they expect an angle in radians.
        sun.position = new Vector3(
            camPos.x + Mathf.Round(_x + _r * Mathf.Cos(_a * Mathf.Deg2Rad)),
            camPos.y + Mathf.Round(_y - _r * Mathf.Sin(_a * Mathf.Deg2Rad)),
            0
        );
    }

    void f_grow() {
        for (int i = 0; i < items.soils.Count; i++)
        {
            var soil = items.soils[i].GetComponent<Soil>();
            if (soil.filled && soil.stage < 3) {
                soil.mini_stage += Random.Range(0, 0.5f);
                if (soil.mini_stage > 1) {
                    soil.mini_stage = 0;
                    soil.stage++;
                }
            }
        }
    }
}
