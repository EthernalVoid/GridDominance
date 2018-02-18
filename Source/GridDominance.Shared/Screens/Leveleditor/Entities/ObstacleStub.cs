﻿using System;
using System.Collections.Generic;
using System.Globalization;
using GridDominance.Levelfileformat.Blueprint;
using GridDominance.Shared.Resources;
using GridDominance.Shared.Screens.Common;
using GridDominance.Shared.Screens.NormalGameScreen.Entities;
using GridDominance.Shared.Screens.NormalGameScreen.Fractions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoSAMFramework.Portable.BatchRenderer;
using MonoSAMFramework.Portable.BatchRenderer.TextureAtlases;
using MonoSAMFramework.Portable.ColorHelper;
using MonoSAMFramework.Portable.DebugTools;
using MonoSAMFramework.Portable.GameMath;
using MonoSAMFramework.Portable.GameMath.Geometry;
using MonoSAMFramework.Portable.Input;
using MonoSAMFramework.Portable.Language;
using MonoSAMFramework.Portable.LogProtocol;
using MonoSAMFramework.Portable.Screens;
using MonoSAMFramework.Portable.Screens.Entities;

namespace GridDominance.Shared.Screens.Leveleditor.Entities
{
	public class ObstacleStub : GameEntity, ILeveleditorStub
	{
		public enum ObstacleStubType { BlackHole=0, WhiteHole=1, GlassBlock=2, MirrorBlock=3, MirrorCircle=4, VoidVircle=5 }

		public static readonly float[] ROTS =
		{
			(0 * 4 + 0) * (FloatMath.TAU/16), (0 * 4 + 1) * (FloatMath.TAU/16), (0 * 4 + 2) * (FloatMath.TAU/16), (0 * 4 + 3) * (FloatMath.TAU/16),
			(1 * 4 + 0) * (FloatMath.TAU/16), (1 * 4 + 1) * (FloatMath.TAU/16), (1 * 4 + 2) * (FloatMath.TAU/16), (1 * 4 + 3) * (FloatMath.TAU/16),
			(2 * 4 + 0) * (FloatMath.TAU/16), (2 * 4 + 1) * (FloatMath.TAU/16), (2 * 4 + 2) * (FloatMath.TAU/16), (2 * 4 + 3) * (FloatMath.TAU/16),
			(3 * 4 + 0) * (FloatMath.TAU/16), (3 * 4 + 1) * (FloatMath.TAU/16), (3 * 4 + 2) * (FloatMath.TAU/16), (3 * 4 + 3) * (FloatMath.TAU/16),
		};

		public static readonly string[] ROT_STR =
		{
			"E", "SEE", "SE", "SSE", "S", "SSW", "SW", "SWW", "W", "NWW", "NW", "NNW", "N", "NNE", "NE", "NEE"
		};

		public readonly TextureRegion2D[] TypeTextures = new[]
		{
			Textures.TexBlackHoleIcon,
			Textures.TexWhiteHoleIcon,
			Textures.TexGlassBlockIcon,
			Textures.TexMirrorBlockIcon,
			Textures.TexMirrorCircleSmall,
			Textures.TexVoidCircle_FG,
		};

		private LevelEditorScreen GDOwner => (LevelEditorScreen) Owner;

		public FPoint Center;
		public float Rotation;
		public float Width;
		public float Height;
		public float Power;
		public ObstacleStubType ObstacleType;

		public override FPoint Position => Center;
		public override FSize DrawingBoundingBox => new FSize(Width, Height);

		public override Color DebugIdentColor => Color.Red;

		public ObstacleStub(GameScreen scrn, FPoint pos, ObstacleStubType t, float w, float h, float r) : base(scrn, GDConstants.ORDER_GAME_CANNON)
		{
			Center       = pos;
			Rotation     = r;
			Width        = w;
			Height       = h;
			Power        = BlackHoleBlueprint.DEFAULT_POWER_FACTOR;
			ObstacleType = t;
		}

		public override void OnInitialize(EntityManager manager) { }
		public override void OnRemove() { }
		protected override void OnUpdate(SAMTime gameTime, InputState istate) { }

		protected override void OnDraw(IBatchRenderer sbatch)
		{
			if (GDOwner.Selection == this) sbatch.DrawCentered(Textures.TexPixel, Position, Width + GDConstants.TILE_WIDTH, Height + GDConstants.TILE_WIDTH, Color.Black*0.333f);

			switch (ObstacleType)
			{
				case ObstacleStubType.BlackHole:
					CommonObstacleRenderer.DrawBlackHole(sbatch, Position, Lifetime, Width, Power);
					break;

				case ObstacleStubType.WhiteHole:
					CommonObstacleRenderer.DrawBlackHole(sbatch, Position, Lifetime, Width, -Power);
					break;

				case ObstacleStubType.GlassBlock:
					CommonObstacleRenderer.DrawGlassBlock(sbatch, FRotatedRectangle.CreateByCenter(Position, Width, Height, Rotation));
					break;

				case ObstacleStubType.MirrorBlock:
					CommonObstacleRenderer.DrawMirrorBlock(sbatch, FRotatedRectangle.CreateByCenter(Position, Width, Height, Rotation));
					break;

				case ObstacleStubType.MirrorCircle:
					CommonObstacleRenderer.DrawMirrorCircle(sbatch, Position, Width);
					break;

				case ObstacleStubType.VoidVircle:
					CommonObstacleRenderer.DrawVoidCircle_BG(sbatch, Position, Width);
					break;

				default:
					SAMLog.Error("LEOS::EnumSwitch_CS_OD", "ObstacleType = " + ObstacleType);
					break;
			}
		}

		protected override void OnDrawOrderedForegroundLayer(IBatchRenderer sbatch)
		{
			switch (ObstacleType)
			{
				case ObstacleStubType.BlackHole:
					// NOP
					break;

				case ObstacleStubType.WhiteHole:
					// NOP
					break;

				case ObstacleStubType.GlassBlock:
					break;

				case ObstacleStubType.MirrorBlock:
					// NOP
					break;

				case ObstacleStubType.MirrorCircle:
					// NOP
					break;

				case ObstacleStubType.VoidVircle:
					CommonObstacleRenderer.DrawVoidCircle_FG(sbatch, Position, Width);
					break;

				default:
					SAMLog.Error("LEOS::EnumSwitch_CS_ODOFL", "ObstacleType = " + ObstacleType);
					break;
			}
		}

#if DEBUG
		protected override void DrawDebugBorders(IBatchRenderer sbatch)
		{
			base.DrawDebugBorders(sbatch);

			//
		}
#endif

		public bool CollidesWith(CannonStub other)
		{
			return this.GetArea().Overlaps(other.GetArea());
		}

		public bool CollidesWith(ObstacleStub other)
		{
			if (ObstacleType == ObstacleStubType.BlackHole || ObstacleType == ObstacleStubType.WhiteHole)
			{
				if (this.Position == other.Position && (other.ObstacleType == ObstacleStubType.BlackHole || other.ObstacleType == ObstacleStubType.WhiteHole)) return true;
				return false;
			}
			if (other.ObstacleType == ObstacleStubType.BlackHole || other.ObstacleType == ObstacleStubType.WhiteHole)
			{
				return false;
			}

			if (this.ObstacleType == ObstacleStubType.VoidVircle && other.ObstacleType == ObstacleStubType.VoidVircle && this.Position != other.Position) return false;
			if (this.ObstacleType == ObstacleStubType.MirrorBlock && other.ObstacleType == ObstacleStubType.MirrorBlock && this.Position != other.Position) return false;

			var g1 = this.GetArea();
			var g2 = other.GetArea();

			return g1.Overlaps(g2);
		}

		public IEnumerable<SingleAttrOption> AttrOptions
		{
			get
			{
				yield return new SingleAttrOption
				{
					Action = ChangeObstacleType,
					Description = L10NImpl.STR_LVLED_BTN_TYPE,
					Icon = () => TypeTextures[(int)ObstacleType],
					Text = () => null,
					TextColor = () => FlatColors.Foreground,
				};
			}
		}

		private void ChangeObstacleType()
		{
			ObstacleType = (ObstacleStubType)(((int)ObstacleType + 1) % 6);

			var idxCurr = (int)ObstacleType;

			for (int i = 1; i < 6; i++)
			{
				var ttest = (ObstacleStubType)((idxCurr + 1) % 6);
				if (GDOwner.CanInsertObstacleStub(Position, ttest, Width, Height, Rotation, this) != null)
				{
					ObstacleType = ttest;
					GDOwner.GDHUD.AttrPanel.Recreate(this);
					return;
				}
			}
		}

		public void Kill()
		{
			Remove();
		}


		public IFShape GetClickArea() => GetArea();

		public IFShape GetArea()
		{
			switch (ObstacleType)
			{
				case ObstacleStubType.BlackHole:
				case ObstacleStubType.WhiteHole:
				case ObstacleStubType.MirrorCircle:
				case ObstacleStubType.VoidVircle:
					return new FCircle(Position, Width);

				case ObstacleStubType.GlassBlock:
				case ObstacleStubType.MirrorBlock:
					return new FRotatedRectangle(Position, Width, Height, Rotation);

				default:
					SAMLog.Error("LEOS::EnumSwitch_IC", "ObstacleType = " + ObstacleType);
					return null;
			}
		}
	}
}
