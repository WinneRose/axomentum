using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    #region Cooldown Timers
    private float lastIcePlatformTime = -Mathf.Infinity;
    [SerializeField] private float icePlatformCooldown = 1.5f;

    private float lastGroundPlatformTime = -Mathf.Infinity;
    [SerializeField] private float groundPlatformCooldown = 1.5f;

    private float lastDoubleJumpTime = -Mathf.Infinity;
    [SerializeField] private float doubleJumpCooldown = 1.5f;
    #endregion

    #region Prefabs and VFX
    [SerializeField] private GameObject groundPlatformPrefab;
    [SerializeField] private GameObject icePlatformPrefab;
    [SerializeField] private GameObject platformDestroyPSPrefab;
    #endregion

    #region Ability UI Icons
    [SerializeField] private Sprite[] dashIcons = new Sprite[2];
    [SerializeField] private Sprite[] icePlatformIcons = new Sprite[2];
    [SerializeField] private Sprite[] groundPlatformIcons = new Sprite[2];
    [SerializeField] private Sprite[] doubleJumpIcons = new Sprite[2];

    [SerializeField] private Image dashIconImage;
    [SerializeField] private Image icePlatformIconImage;
    [SerializeField] private Image groundPlatformIconImage;
    [SerializeField] private Image doubleJumpIconImage;
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
            if (CanUseDoubleJump())
            {
                _playerController.canDoubleJump = true;
                lastDoubleJumpTime = Time.time;
            }
        }

        UpdateCooldownVisuals();
    }

    private void InitializeElementsForScene(string sceneName)
    {
        activeElements.Clear();

        switch (sceneName)
        {
            case "scn_fireBiome":
                activeElements.AddRange(new[] { Elements.Air, Elements.Ground });
                break;
            case "scn_waterBiome":
                activeElements.AddRange(new[] { Elements.Air, Elements.Ground });
                break;
            case "scn_airBiome":
                activeElements.AddRange(new[] { Elements.Water, Elements.Fire });
                break;
            case "scn_groundBiome":
                activeElements.AddRange(new[] { Elements.Water, Elements.Fire });
                break;
            case "scn_testScene":
                activeElements.AddRange(new[] { Elements.Water, Elements.Air, Elements.Fire });
                break;
            default:
                Debug.LogWarning("No elements defined for this scene.");
                break;
        }

        foreach (var element in activeElements)
            Debug.Log($"Activated: {element}");

        RefreshAbilityIcons();
    }

    private void RefreshAbilityIcons()
    {
        dashIconImage.sprite = IsElementActive(Elements.Air) ? dashIcons[1] : dashIcons[0];
        doubleJumpIconImage.sprite = IsElementActive(Elements.Fire) ? doubleJumpIcons[1] : doubleJumpIcons[0];
        groundPlatformIconImage.sprite = IsElementActive(Elements.Ground) ? groundPlatformIcons[1] : groundPlatformIcons[0];
        icePlatformIconImage.sprite = IsElementActive(Elements.Water) ? icePlatformIcons[1] : icePlatformIcons[0];
    }

    private void UpdateCooldownVisuals()
    {
        float dashRemaining = Mathf.Clamp01((_playerController.DashCooldown - (Time.time - _playerController.LastDashTime)) / _playerController.DashCooldown);
        dashIconImage.color = new Color(1f, 1f, 1f, 1f - dashRemaining);

        float iceRemaining = Mathf.Clamp01((icePlatformCooldown - (Time.time - lastIcePlatformTime)) / icePlatformCooldown);
        icePlatformIconImage.color = new Color(1f, 1f, 1f, 1f - iceRemaining);

        float groundRemaining = Mathf.Clamp01((groundPlatformCooldown - (Time.time - lastGroundPlatformTime)) / groundPlatformCooldown);
        groundPlatformIconImage.color = new Color(1f, 1f, 1f, 1f - groundRemaining);

        float djRemaining = Mathf.Clamp01((doubleJumpCooldown - (Time.time - lastDoubleJumpTime)) / doubleJumpCooldown);
        doubleJumpIconImage.color = new Color(1f, 1f, 1f, 1f - djRemaining);
    }

    public bool IsElementActive(Elements element)
    {
        return activeElements.Contains(element);
    }

    public void Dash()
    {
        if (!IsElementActive(Elements.Air))
        {
            Debug.Log("Air element not active, can't dash");
            return;
        }

        bool dashed = _playerController.TryDash();
        if (!dashed)
        {
            Debug.Log("Dash is on cooldown");
        }
    }

    public void GroundPlatform()
    {
        if (!IsElementActive(Elements.Ground) || _playerController.isGrounded) return;
        if (!CanUseGroundPlatform() || !CanPlacePlatform()) return;

        lastGroundPlatformTime = Time.time;
        Vector3 spawnPosition = playerTransform.position + Vector3.down * 0.5f;
        GameObject groundPlatform = Instantiate(groundPlatformPrefab, spawnPosition, Quaternion.identity);
        _playerController.ReceiveDamage(2);
        StartCoroutine(DestroyAfterSeconds(groundPlatform));
    }

    public void IcePlatform()
    {
        if (!IsElementActive(Elements.Water) || _playerController.isGrounded) return;
        if (!CanUseIcePlatform() || !CanPlacePlatform()) return;

        lastIcePlatformTime = Time.time;
        Vector3 spawnPosition = playerTransform.position + Vector3.down * 0.5f;
        GameObject icePlatform = Instantiate(icePlatformPrefab, spawnPosition, Quaternion.identity);
        _playerController.ReceiveDamage(2);
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
                Destroy(ps, 3f);
            }
        }
    }

    private void OnMoveKeyStarted(InputAction.CallbackContext context)
    {
        string keyPressed = context.control.name.ToLower();

        Debug.Log("Key pressed: " + keyPressed);

        if (keyPressed == "shift")
            Dash();

        if (keyPressed == "e")
        {
            if (IsElementActive(Elements.Water))
            {
                IcePlatform();
                
            }
                
            else if (IsElementActive(Elements.Ground))
            {
                GroundPlatform();
               
            }
               
        }
    }

    private bool CanUseIcePlatform()
    {
        return Time.time >= lastIcePlatformTime + icePlatformCooldown;
    }

    private bool CanUseGroundPlatform()
    {
        return Time.time >= lastGroundPlatformTime + groundPlatformCooldown;
    }

    private bool CanUseDoubleJump()
    {
        return Time.time >= lastDoubleJumpTime + doubleJumpCooldown;
    }

    private bool CanPlacePlatform()
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
