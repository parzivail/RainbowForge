namespace RainbowForge
{
	public enum Magic : ulong
	{
		Metadata = 0,
		FileContainer = 0x1014FA99,
		CompiledMeshObject = 0xABEB2DFB,
		CompiledMesh = 0xFC9E1595,
		CompiledTextureMap = 0x13237FE9,
		CompiledLowResolutionTextureMap = 0xD7B5C478,
		CompiledMediumResolutionTextureMap = 0xF9C80707,
		CompiledHighResolutionTextureMap = 0x59CE4D13,
		CompiledUltraResolutionTextureMap = 0x9F492D22,
		CompiledFutureResolutionTextureMap = 0x3876CCDF,
		TextureGui1 = 0x9468B9E2,
		TextureGui2 = 0x05A61FAD,
		WemSound = 0x427411A3,
		BuildTable = 0x22ECBE63,
		EntityBuilder = 0x971A842E,
		WeaponData = 0xADBAB640,
		GameBootstrap = 0xE5A83560,
		LocalizationPackage = 0x6E3C9C6F,
		PlatformManager = 0xAE88DE65,
		World = 0xFBB63E47,
		LoadUnit = 0x943945C4,
		WorldMetaData = 0x3E237DA3,
		GIStream = 0xD16E3EBE,
		CompiledMeshShapeDataObject = 0x9231EE0F,
		FlatArchive12 = 0x82688E42,
		ShaderCodeModuleUserMaterial = 0x1C9A0555,
		Material = 0x85C817C3,
		TextureMapSpec = 0x989DC6B2,
		TextureMap = 0xA2B7E917,
		Mesh = 0x415D9568,
		Skeleton = 0x24AECB7C,
		CollisionMaterial = 0x74F7311D,
		Entity = 0x0984415E,
		SpotLight = 0x80320FB8,
		FacialPoseGroup = 0xE640B4DA,
		FX = 0x824A23BA,
		SplashFX = 0x755ACE14,
		BallJointCommonData = 0x460DD209,
		LiteRagdoll = 0x891043D5,
		BoxShape = 0x4EC68E98,
		BuildRow = 0x348B28D6, // BuildRow in a BuildTable
		FireFontDescriptor = 0x58E38D86,
		MeshShape = 0xB22B3E61,
		PostEffects = 0xC83B8907,
		AtomGraph = 0xC0C861CD,
		BarrelShape = 0x97CD8890,
		BodyPartMapping = 0xB3ADF542,
		CapsuleShape = 0xB8599052,
		ClutterGenerationSettings = 0xB144F3A0,
		ConvexVerticesShape = 0x53667A87,
		EntityGroup = 0x3F742D26,
		GameSetting = 0x52534498,
		ListShape = 0x86EBFD8D,
		LODSelector = 0x51DC6B80,
		MarketingCamera = 0x5B7FC715,
		OmniLight = 0x344780D6,
		PersistableOptionsProfile = 0x8F07EDFF,
		RagdollNew = 0xC8DBDE7F,
		SoundPropagationMap = 0x97FCF21E,
		SphereShape = 0xFA3F7A18,
		Gadget = 0xB1F352BC,
		Weapon = 0x6E9FA2D0,
		Charm = 0x40FCADEE,
		Projectile = 0x73A641BD,
		Animation = 0x0FA3067F,
		AreaMap = 0x584879B2,
		TheaterData = 0x84EC653F,
		KinoExternalStateData = 0xB19D4FCD,
		KinoReplaceSetData = 0x3F49D5C9,
		KinoGraphData = 0x3527D1AE,
		TheaterCinematic = 0x28435F00,
		KinoMarkupDictionaryData = 0xBBA2898E,
		KinoReplaceFamilyData = 0x28941138,
		KinoReplaceTreeData = 0x2C1329D2,
		DominoScriptDefinition = 0xE802B9DA,
		CreditsData = 0x88AEAEB2,
		KinoBootStrapData = 0x0549696C,
		Universe = 0x98435A63,
		KinoMarkupSystemData = 0xABD50170,
		KinoSyncSystemData = 0x72FCB11D,
		KinoTagSystemData = 0x92F31FA2,
		GraphicsConfig = 0xB1420AD1,
		FactionSettings = 0x5CB07EA0,
		DebugSettings = 0x9B7A3615,
		AnimationSettings = 0x174B8004,
		KinoRuntimeData = 0x149E0A31,
		KinoReplaceSystemData = 0x235C0560,
		MultiLodSetup = 0xAEC77F9A,
		BulletPenetrationSettings = 0xFB75DFB2,
		ShaderTemplate = 0x6F74DDB4,
		DamageData = 0x314AECEE,
		WeaponSoundData = 0xBF95D157,
		DestructionMaterial = 0x6A7D34A9,
		ArmorData = 0x7DC13543,
		ProjectileData = 0xA2D01604,
		GadgetData = 0xB6AA66C5,
		ReinforcementGadgetData = 0xB93410FB,
		BodyPartTemplate = 0x02251ED8,
		LocalizationComponent = 0x8DC233A0,
		R6GMGameMode = 0xC56C24BF, //gamemodes
		GMPhase = 0x195883E9, //gamemode parameters
		TagValue = 0x96EADACB, //more gamemode parameters
		Character = 0x118B3297, //operators
		CharacterSet = 0xB11FC381, //elite sets
		CharacterHeadgear = 0xF080B73C,
		CharacterUniform = 0x74DF1F46,
		WeaponSet = 0xAF5106DC,
		GadgetSkin = 0x1920FDB8,
		WeaponSkin = 0x02302D3D,
		WeaponAttachmentSkin = 0xF827AF5A,
		WeaponAttachmentSkinSet = 0x33E7DDEF

		// 0x6F74DDB4 - Shader?
		// 0x8D1D966B - some kind of UID mapping container
		// 0x4CAED6B4 - another UID mapping container
	}

	public enum ContainerMagic : uint
	{
		Descriptor = 1,
		Hash = 6,
		File = 0x57FBAA34
	}

	public enum AssetType
	{
		Unknown,
		Mesh,
		Texture,
		Sound,
		FlatArchive
	}
}