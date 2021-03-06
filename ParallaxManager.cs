﻿using System.Collections.Generic;
using UnityEngine;
using YoukaiFox.Math;
using YoukaiFox.UnityExtensionMethods;
using YoukaiFox.Inspector;
using YoukaiFox.Parallax.Helpers;

namespace YoukaiFox.Parallax
{
    public class ParallaxManager : MonoBehaviour
    {
        #region Static fields

        public static ParallaxManager Instance;

        #endregion

        #region Serialized Fields

        [SerializeField]
        private SortingMethod _sortingMethod;

        [SerializeField]
        private int _reservedIndicesQnty = 3;

        [SerializeField] 
        [HideIf(nameof(UseSpriteSorting))]
        private int _initialSortingIndex = 0;

        [SerializeField]
        // [ReorderableList] 
        [ReadOnly]
        private List<ParallaxElement> _parallaxElements;

        #endregion

        #region Non-Serialized Fields

        private Camera _mainCamera;
        private Transform _mainCameraTransform;
        private Vector3 _previousCameraPosition;
        private Vector3 _currentCameraPosition;
        private Vector3 _currentCameraDisplacement;
        private EDirection _cameraMovementDirection;
        private float _lowestZvalueAvailable;
        private int _reservedSortOrderIndex;

        #endregion

        #region Properties

        public Vector3 CurrentCameraPosition => _currentCameraPosition;
        public Vector3 CurrentCameraDisplacement => _currentCameraDisplacement;
        public EDirection MovementDirection => _cameraMovementDirection;
        public int ReservedSortOrderIndex => _reservedSortOrderIndex;
        private bool UseSpriteSorting => _sortingMethod == SortingMethod.SpriteSorting;
        private bool UseZAxisSorting => _sortingMethod == SortingMethod.ZAxis;

        #endregion

        #region Custom structures

        public enum SortingMethod
        {
            ZAxis, SpriteSorting
        }
        #endregion

        #region Unity Methods

        private void Awake()
        {
            SetupSingleton();
            ReferenceComponents();
        }

        private void Start()
        {
            AddChildrenElements();
            SortChildrenElements();
            ConfigureSortingOrder();
            _previousCameraPosition = _mainCameraTransform.position;
            _currentCameraPosition = _mainCameraTransform.position;
            CalculateLowestZAxisValueAvailable();
        }

        private void Update()
        {
            UpdateCameraPosition();
            UpdateCameraDisplacement();
            UpdateCameraMovementDirection();
            UpdatePreviousCameraPosition();
        }

        #endregion

        #region Public Methods

        public Bounds GetCameraBounds()
        {
            return _mainCamera.OrthographicBounds();
        }

        public float GetLowestZAxisValueAvailable()
        {
            return _lowestZvalueAvailable;
        }

        #endregion

        #region Private Methods

        private void AddChildrenElements()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var element = transform.GetChild(i).GetComponent<ParallaxElement>();

                if (element)
                    _parallaxElements.Add(element);
                else
                    transform.GetChild(i).SetParent(null, false);
            }
        }

        private void SortChildrenElements()
        {
            _parallaxElements.Sort();
        }

        private void ConfigureSortingOrder()
        {
            switch (_sortingMethod)
            {
                case SortingMethod.ZAxis:
                    ConfigureZAxisSorting();
                    break;
                case SortingMethod.SpriteSorting:
                    ConfigureSpriteSorting();
                    break;
                default:
                    throw new System.Exception();
            }
        }

        private void ConfigureSpriteSorting()
        {
            bool finishedReadingBgElements = false;

            for (int i = 0; i < _parallaxElements.Count + _reservedIndicesQnty; i++)
            {
                if (_parallaxElements[i].GetPlane() == ParallaxLayeredElement.Plane.Background)
                {
                    _reservedSortOrderIndex = i + 1;
                }
                else
                {
                    if (!finishedReadingBgElements)
                    {
                        finishedReadingBgElements = true;
                        _reservedSortOrderIndex = i;
                        i += _reservedIndicesQnty;
                    }
                }

                if ((!finishedReadingBgElements) && (i >= _parallaxElements.Count))
                    return;

                _parallaxElements[i].SetSortingOrder(i);
            }
        }

        private void ConfigureZAxisSorting()
        {
            foreach (var element in _parallaxElements)
            {
                if (element.GetPlane() == ParallaxLayeredElement.Plane.Background)
                    element.SetSortingOrder(_initialSortingIndex);
                else
                    element.SetSortingOrder(_initialSortingIndex + _reservedSortOrderIndex);
            }

            _reservedSortOrderIndex = _initialSortingIndex + 1;
        }

        private void UpdateCameraPosition()
        {
            _currentCameraPosition = _mainCameraTransform.position;
        }

        private void UpdateCameraDisplacement()
        {
            _currentCameraDisplacement = _currentCameraPosition - _previousCameraPosition;
        }

        private void UpdateCameraMovementDirection()
        {
            if (_currentCameraPosition.x > _previousCameraPosition.x)
            {
                _cameraMovementDirection = EDirection.Right;
            }
            else if (_currentCameraPosition.x < _previousCameraPosition.x)
            {
                _cameraMovementDirection = EDirection.Left;
            }
            else
            {
                _cameraMovementDirection = EDirection.None;
            }
        }

        private void UpdatePreviousCameraPosition()
        {
            _previousCameraPosition = _currentCameraPosition;
        }

        private void CalculateLowestZAxisValueAvailable()
        {
            _lowestZvalueAvailable = YoukaiMath.Abs(Mathf.Floor(_mainCameraTransform.position.z + 1f));
        }

        private void ReferenceComponents()
        {
            _mainCamera = Camera.main;
            _mainCameraTransform = Camera.main.transform;
        }
        
        private void SetupSingleton()
        {
            if (Instance == null)
                Instance = this;
            else
                throw new System.Exception();
        }

        #endregion
    }
}
