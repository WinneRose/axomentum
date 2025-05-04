using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum Elements { Fire, Water, Air, Ground }

public class AbilityManager : MonoBehaviour
{
    #region Components
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private InputAction _inputAction;
    [SerializeField] private Transform playerTransform;
    #endregion

    #region Variables
    [SerializeField] private List<Elements> activeElements = new List<Elements>();
    private float lastPlacedPlatform = -Mathf.Infinity;
    [SerializeField] private float PlatformPlaceCooldown = 1.5f;
    #endregion

    #region Prefabs and VFX
    [SerializeField] private GameObject groundPlatformPrefab;
    [SerializeField] private GameObject icePlatformPrefab;
    [SerializeField] private GameObject platformDestroyPSPrefab;
    #endregion

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        InitializeElementsForScene(SceneManager.GetActiveScene().name);
    }

    private void OnEnable()
    {
        _inputAction.Enable();
        _inputAction.started += OnMoveKeyStarted;
    }

    private void OnDisable()
    {
        _inputAction.started -= OnMoveKeyStarted;
        _inputAction.Disable();
    }

    private void Update()
    {
        if (_playerController.isGrounded && IsElementActive(Elements.Fire))
        {
            _playerController.canDoubleJump = true;
        }
    }

    private void InitializeElementsForScene(string sceneName)
    {
        activeElements.Clear();

        switch (sceneName)
        {
            case "scn_fireBiome":
                activeElements.Add(Elements.Fire);
                break;
            case "scn_waterBiome":
                activeElements.Add(Elements.Air);
                break;
            case "scn_airBiome":
                activeElements.Add(Elements.Water);
                break;
            case "scn_groundBiome":
                activeElements.Add(Elements.Ground);
                break;
            case "scn_testScene":
                activeElements.AddRange(new[] { Elements.Water });
                break;
            default:
                Debug.LogWarning("No elements defined for this scene.");
                break;
        }

        foreach (var element in activeElements)
            Debug.Log($"Activated: {element}");
    }

    public bool IsElementActive(Elements element)
    {
        return activeElements.Contains(element);
    }

    public void Dash()
    {
        if (IsElementActive(Elements.Air))
        {
            bool dashed = _playerController.TryDash();
            if (!dashed)
                Debug.Log("Dash is on cooldown");
        }
        else
        {
            Debug.Log("Air element not active, can't dash");
        }
    }

    public void GroundPlatform()
    {
        if (!IsElementActive(Elements.Ground)) return;

        Vector3 spawnPosition = playerTransform.position + Vector3.down * 1.5f;
        GameObject groundPlatform = Instantiate(groundPlatformPrefab, spawnPosition, Quaternion.identity);
        StartCoroutine(DestroyAfterSeconds(groundPlatform));
    }

    public void IcePlatform()
    {
        if (!IsElementActive(Elements.Water)) return;

        Vector3 spawnPosition = playerTransform.position + Vector3.down * 1.5f;
        GameObject icePlatform = Instantiate(icePlatformPrefab, spawnPosition, Quaternion.identity);
        StartCoroutine(DestroyAfterSeconds(icePlatform));
    }

    private IEnumerator DestroyAfterSeconds(GameObject platform)
    {
        yield return new WaitForSeconds(5f);

        if (platform != null)
        {
            Vector3 pos = platform.transform.position;
            Destroy(platform);

            if (platformDestroyPSPrefab != null)
            {
                GameObject ps = Instantiate(platformDestroyPSPrefab, pos, Quaternion.identity);
                Destroy(ps, 3f); // optional: auto destroy particle
            }
        }
    }

    private void OnMoveKeyStarted(InputAction.CallbackContext context)
    {
        string keyPressed = context.control.name.ToLower();

        Debug.Log("Key pressed: " + keyPressed);

        if (keyPressed == "shift")
            Dash();

        if (CanPlacePlatform())
        {
            if (keyPressed == "e" && IsElementActive(Elements.Water))
            {
                IcePlatform();
            }
            
            else if (keyPressed == "e" && IsElementActive(Elements.Ground))
            {
                GroundPlatform();
            }
        }
        
        
            
    }

    bool CanPlacePlatform()
    {
        if (Time.time > lastPlacedPlatform + PlatformPlaceCooldown)
        {
            lastPlacedPlatform = Time.time;
            Debug.Log("Platform cooldown finished and can placeable");
            return true;
        }
        return false;
    }
}
