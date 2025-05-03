using System;
using UnityEngine;
using System.Collections;

public class ClosingTrap : MonoBehaviour
{
    [SerializeField] private GameObject _closingTrap;
    [SerializeField] private Transform _closingTrapLocation;
    [SerializeField] private float trapMoveSpeed = 2f;

    private bool _shouldMoveTrap = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _closingTrap.SetActive(true);
            _shouldMoveTrap = true;
            
        }
    }
    

    private void Update()
    {
        if (_shouldMoveTrap && _closingTrap != null)
        {
            _closingTrap.transform.position = Vector2.MoveTowards(
                _closingTrap.transform.position,
                _closingTrapLocation.position,
                trapMoveSpeed * Time.deltaTime
            );

            // Optional: stop moving if close enough
            if (Vector2.Distance(_closingTrap.transform.position, _closingTrapLocation.position) < 0.01f)
            {
                _shouldMoveTrap = false;
            }
        }
    }
    
}