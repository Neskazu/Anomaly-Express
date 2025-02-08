using Player;
using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerAudioController : NetworkBehaviour
{
    [SerializeField]
    PlayerController playerController;
    [SerializeField]
    private AudioSource footAudioSource;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(!IsOwner)
        {
            return;
        }
        playerController.OnMovementStateChanged += HandleFootstepSound;
        footAudioSource.Play();
        footAudioSource.Pause();
    }

    private void HandleFootstepSound(bool isMoving)
    {
        if (isMoving)
        {
            footAudioSource.UnPause();
        }
        else
        {
            footAudioSource.Pause();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
