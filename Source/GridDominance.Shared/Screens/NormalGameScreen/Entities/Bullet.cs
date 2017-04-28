﻿using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using GridDominance.Shared.Resources;
using GridDominance.Shared.Screens.NormalGameScreen.EntityOperations;
using Microsoft.Xna.Framework;
using MonoSAMFramework.Portable.BatchRenderer;
using MonoSAMFramework.Portable.Input;
using MonoSAMFramework.Portable.GameMath;
using GridDominance.Shared.Screens.NormalGameScreen.Fractions;
using MonoSAMFramework.Portable.GameMath.Geometry;
using MonoSAMFramework.Portable.GameMath.Geometry.Alignment;
using MonoSAMFramework.Portable.LogProtocol;
using MonoSAMFramework.Portable.Screens;
using MonoSAMFramework.Portable.Screens.Entities;

namespace GridDominance.Shared.Screens.NormalGameScreen.Entities
{
	class Bullet : GameEntity
	{
		public  const float BULLET_DIAMETER = 25;
		private const float MAXIMUM_LIEFTIME = 25;

		public readonly Fraction Fraction;

		public bool IsDying;

		public Vector2 BulletPosition;
		public float BulletRotation = 0f;
		public float BulletAlpha = 1f;
		public float BulletExtraScale = 1f;

		public Body PhysicsBody;
		public readonly Cannon Source;
		public readonly float Scale;
		
		private readonly Vector2 initialVelocity;

		public override Vector2 Position => BulletPosition;
		public override FSize DrawingBoundingBox { get; }
		public override Color DebugIdentColor => Fraction.Color;

		public Bullet(GameScreen scrn, Cannon shooter, Vector2 pos, Vector2 velo, float entityScale) : base(scrn, GDConstants.ORDER_GAME_BULLETS)
		{
			BulletPosition = pos;
			initialVelocity = velo;
			Source = shooter;
			Fraction = Source.Fraction;
			Scale = entityScale;

			DrawingBoundingBox = new FSize(BULLET_DIAMETER, BULLET_DIAMETER);
		}

		public override void OnInitialize(EntityManager manager)
		{
			PhysicsBody = BodyFactory.CreateCircle(this.GDManager().PhysicsWorld, ConvertUnits.ToSimUnits(Scale * BULLET_DIAMETER / 2), 1, ConvertUnits.ToSimUnits(BulletPosition), BodyType.Dynamic, this);
			PhysicsBody.LinearVelocity = ConvertUnits.ToSimUnits(initialVelocity);
			PhysicsBody.CollidesWith = Category.All;
			PhysicsBody.IsBullet = true;
			PhysicsBody.Restitution = 0.95f;
			PhysicsBody.AngularDamping = 0.5f;
			PhysicsBody.Friction = 0.2f;
			PhysicsBody.LinearDamping = 0f;
			PhysicsBody.OnCollision += OnCollision;
			PhysicsBody.AngularVelocity = 0; //TODO Remove all angular calculations? (-> more consistency and faster calc?)
			//Body.Mass = Scale * Scale; // Weight dependent on size
		}

		private bool OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
		{
			if (!Alive || IsDying) return false;

			var otherBullet = fixtureB.UserData as Bullet;
			if (otherBullet != null)
			{
				if (otherBullet.Fraction == Fraction) return true;

				otherBullet.MutualDestruct();
				MutualDestruct();
				MainGame.Inst.GDSound.PlayEffectCollision();
				return false;
			}
			
			var otherCannon = fixtureB.UserData as Cannon;
			if (otherCannon != null)
			{

				if (otherCannon.Fraction == Fraction)
				{
					// if Source barrel then ignore collision
					if (otherCannon != Source || fixtureB == otherCannon.PhysicsFixtureBase)
					{
						DisintegrateIntoFriend();
						otherCannon.ApplyBoost();
						MainGame.Inst.GDSound.PlayEffectBoost();
					}
				}
				else // if (otherCannon.Fraction != this.Fraction)
				{
					DisintegrateIntoEnemy();
					otherCannon.TakeDamage(Fraction, Scale);
					MainGame.Inst.GDSound.PlayEffectHit();
				}

				return false;
			}

			var otherVoidWall = fixtureB.UserData as VoidWall;
			if (otherVoidWall != null)
			{
				DisintegrateIntoVoidObject();
				MainGame.Inst.GDSound.PlayEffectCollision();
				return false;
			}

			var otherVoidCircle = fixtureB.UserData as VoidCircle;
			if (otherVoidCircle != null)
			{
				DisintegrateIntoVoidObject();
				MainGame.Inst.GDSound.PlayEffectCollision();
				return false;
			}

			// wud ???
			SAMLog.Error("Collision", string.Format("Bullet collided with unkown fixture: {0}", fixtureB.UserData ?? "<NULL>"));
			return false;
		}

		private void MutualDestruct()
		{
			// After Bullet-Bulllet Collision

			if (MainGame.Inst.Profile.EffectsEnabled)
			{
				for (int i = 0; i < 8; i++)
					Manager.AddEntity(new BulletSplitter(Owner, this, (FlatAlign8) i));
			}

			Alive = false;
		}

		private void DisintegrateIntoVoidObject()
		{
			// After Bullet-Void Collision

			if (MainGame.Inst.Profile.EffectsEnabled)
			{
				for (int i = 0; i < 8; i++)
					Manager.AddEntity(new BulletSplitter(Owner, this, (FlatAlign8)i));
			}

			Alive = false;
		}

		private void DisintegrateIntoEnemy()
		{
			// After Bullet-Cannon Collision

			AddEntityOperation(new BulletDamageAndDieOperation());
			IsDying = true;
		}

		private void DisintegrateIntoFriend()
		{
			// After Bullet-Cannon Collision

			AddEntityOperation(new BulletConsumeAndDieOperation());
			IsDying = true;
		}

		public override void OnRemove()
		{
			this.GDManager().PhysicsWorld.RemoveBody(PhysicsBody);
		}

		protected override void OnUpdate(SAMTime gameTime, InputState istate)
		{
			BulletPosition = ConvertUnits.ToDisplayUnits(PhysicsBody.Position);
			BulletRotation = PhysicsBody.Rotation;

			if (Lifetime > MAXIMUM_LIEFTIME) AddEntityOperation(new BulletFadeAndDieOperation());

			if (!Manager.BoundingBox.Contains(BulletPosition)) Remove();
		}

		protected override void OnDraw(IBatchRenderer sbatch)
		{
			sbatch.DrawCentered(
				Textures.TexBullet, 
				BulletPosition, 
				Scale * BulletExtraScale * BULLET_DIAMETER,
				Scale * BulletExtraScale * BULLET_DIAMETER,
				Fraction.Color * BulletAlpha, BulletRotation);
		}
	}
}