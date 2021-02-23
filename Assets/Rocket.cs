using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour {

    [SerializeField] float rcsThrust = 100f;
    [SerializeField] float mainThrust = 100f;
    [SerializeField] float levelLoadDelay = 2f;

    [SerializeField] AudioClip mainEngine;
    [SerializeField] AudioClip success;
    [SerializeField] AudioClip death;

    [SerializeField] ParticleSystem mainEngineParticles;
    [SerializeField] ParticleSystem successParticles;
    [SerializeField] ParticleSystem deathParticles;

    bool isTransitioning = false;
    bool collisions = true;
    
    Rigidbody rigidBody;
    AudioSource audioSource;

    // Start is called before the first frame update
    void Start() {
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update() {
        if (!isTransitioning) {
            RespondToThrustInput();
            RespondToRotateInput();
        }
        if (Debug.isDebugBuild) {
            RespondToDebugKeys();
        }
    }

    void OnCollisionEnter(Collision collision) {
        if (isTransitioning|| !collisions) {
            return;
        }
        switch (collision.gameObject.tag) {
            case "Friendly":
                // do nothing
                break;
            case "Finish":
                StartSuccessSequence();
                break;
            default:
                StartDeathSequence();
                break;
        }
    }

    private void StartSuccessSequence() {
        isTransitioning = true;
        audioSource.Stop();
        audioSource.PlayOneShot(success);
        successParticles.Play();
        Invoke("LoadNextLevel", levelLoadDelay);
    }

    private void StartDeathSequence() {
        isTransitioning = true;
        audioSource.Stop();
        audioSource.PlayOneShot(death);
        deathParticles.Play();
        Invoke("LoadFirstLevel", levelLoadDelay);
    }

    private void LoadNextLevel() {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex == (SceneManager.sceneCountInBuildSettings - 1) ? 0 : currentSceneIndex + 1;
        SceneManager.LoadScene(nextSceneIndex);
    }

    private void LoadFirstLevel() {
        SceneManager.LoadScene(0);
    }

    private void RespondToThrustInput() {
        if (Input.GetKey(KeyCode.Space)) { // Can thrust while rotating
            ApplyThrust();
        } else {
            StopApplyingThrust();
        }
    }

    private void StopApplyingThrust() {
        audioSource.Stop();
        mainEngineParticles.Stop();
    }

    private void ApplyThrust() {
        float thrustThisFrame = mainThrust * Time.deltaTime;
        rigidBody.AddRelativeForce(Vector3.up * thrustThisFrame);
        if (!audioSource.isPlaying) {
            audioSource.PlayOneShot(mainEngine);
        }
        if (!mainEngineParticles.isPlaying) {
            mainEngineParticles.Play();
        }
    }

    private void RespondToRotateInput() {
        rigidBody.angularVelocity = Vector3.zero; // Remove rotation due to physics

        float rotationThisFrame = rcsThrust * Time.deltaTime;
        if (Input.GetKey(KeyCode.A)) {
            transform.Rotate(Vector3.forward * rotationThisFrame);
        } else if (Input.GetKey(KeyCode.D)) {
            transform.Rotate(-Vector3.forward * rotationThisFrame);
        }
    }

    private void RespondToDebugKeys() {
        if (Input.GetKey(KeyCode.L)) {
            LoadNextLevel();
        } else if (Input.GetKeyDown(KeyCode.C)) {
            collisions = !collisions;
            print("Switching collissions to " + collisions);
        }
    }
}
