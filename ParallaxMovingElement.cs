﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using YoukaiFox.Math;
using YoukaiFox.Inspector;
using YoukaiFox.Parallax.Helpers;

namespace YoukaiFox.Parallax
{
    public class ParallaxMovingElement : ParallaxLayeredElement
    {
        #region Serialized fields

        [SerializeField]
        private MovingPattern _movingPattern;

        [SerializeField] 
        [ShowIf(nameof(IsRandom), false)]
        private EDirection _movementDirection;

        [SerializeField] 
        [ShowIf(nameof(IsRandom), false)]
        private Vector2 _customDirection;

        [SerializeField]
        private float _randomnessStrength = 0.25f;

        [SerializeField]
        private float _movementSpeed = 0.5f;

        [SerializeField]
        [LeftToggle]
        private bool _changesPositionWhenRedrawn = false;

        #endregion

        #region Non-serialized fields

        private Tween _floatingTween;
        private System.Random _random;
        
        #endregion

        #region Properties

        private bool IsLinear => _movingPattern == MovingPattern.Linear;
        private bool IsRandom => _movingPattern == MovingPattern.Random;

        #endregion

        public enum MovingPattern
        {
            Random, Linear
        }

        #region Protected methods

        #region Overridden methods
        protected override void Initialize()
        {
            base.Initialize();
            _random = new System.Random();
        }

        protected override void OnLateUpdateEnter()
        {
            Vector3 nextPosition = CalculateNextPosition();
            Move(nextPosition);
        }

        #endregion

        #endregion

        #region Private methods

        private Vector3 MoveLinearly()
        {
            Vector3 displacement = Vector3.zero;

            switch (_movementDirection)
            {
                case EDirection.Right:
                    displacement.x = _movementSpeed * Time.deltaTime;
                    break;
                case EDirection.Left:
                    displacement.x = -_movementSpeed * Time.deltaTime;
                    break;
                case EDirection.Up:
                    displacement.y = _movementSpeed * Time.deltaTime;
                    break;
                case EDirection.Down:
                    displacement.y = -_movementSpeed * Time.deltaTime;
                    break;
                default:
                    break;
            }

            return displacement;
        }

        private Vector3 MoveRandomly()
        {
            Vector3 randomDirection = YoukaiMath.GetRandomNormalizedVector2();
            randomDirection *= _randomnessStrength;
            randomDirection += InitialPosition;
            return randomDirection * _movementSpeed * Time.deltaTime;
        }

        protected override Vector3 CalculateNextPosition()
        {
            switch (_movingPattern)
            {
                case MovingPattern.Random:
                    return MoveRandomly() + GetParallaxMovement();
                case MovingPattern.Linear:
                    return MoveLinearly() + GetParallaxMovement();
                default:
                    throw new System.ArgumentOutOfRangeException();
            }
        }

        #endregion
    }
}