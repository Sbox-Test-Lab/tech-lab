using Sandbox;

public partial class VideoBlackJack : Component
{
	public enum GameState
	{
		WaitingForBet,
		PlayerTurn,
		DealerTurn,
		GameOver
	}
	
	private Deck Deck;
	public Hand PlayerHand;
	public Hand DealerHand;

	private GameState MiniGameState;

	public int CurrentBet;
	public int PlayerChips = 1000;

	[Property] public WorldPanel WorldPanel;
	[Property] public ModelRenderer MachineModel;

	[Property] CameraComponent SceneCamera;
	private Texture RenderTarget;
	private Material Material;

	protected override void OnStart()
	{
		base.OnStart();

		Deck = new Deck();
		PlayerHand = new Hand();
		DealerHand = new Hand();

		MiniGameState = GameState.WaitingForBet;

		RenderTarget = Texture.CreateRenderTarget()
			.WithSize( 1024, 1024 )
			.WithFormat( ImageFormat.RGBA8888 )
			.Create();
		

		if (MachineModel != null )
		{
			Material = Material.Load( "materials/videoblackjack_screen.vmat" ).CreateCopy();
			Material.Set( "Color", RenderTarget );
			MachineModel.MaterialOverride = Material;
			MachineModel.SetMaterialOverride( Material, "arcade_screen" );
			
			
		}

	}

	protected override void OnUpdate()
	{
		//if ( WorldPanel == null || !SceneCamera.IsValid() || RenderTarget == null ) return;

		
		SceneCamera.RenderToTexture(RenderTarget);
		
		DebugOverlay.Texture( RenderTarget, new Rect( 0, 0, 660, 765 ) );
	}

	public void PlaceBet( int amount )
	{
		NewGame();

		if ( MiniGameState != GameState.WaitingForBet || PlayerChips < amount )
			return;

		CurrentBet += amount;
		PlayerChips -= amount;
	}

	public void DealCards()
	{
		if ( MiniGameState != GameState.WaitingForBet || CurrentBet == 0 )
			return;

		// Clear previous hands
		PlayerHand.Clear();
		DealerHand.Clear();

		// Deal initial cards
		PlayerHand.AddCard( Deck.DrawCard() );
		DealerHand.AddCard( Deck.DrawCard() );
		PlayerHand.AddCard( Deck.DrawCard() );
		DealerHand.AddCard( Deck.DrawCard(true) ); // Dealer's face-down card

		MiniGameState = GameState.PlayerTurn;

		// Check for blackjack
		if ( PlayerHand.IsBlackjack() )
		{
			if ( DealerHand.IsBlackjack() )
			{
				EndGame( "Push! Both have blackjack!" );
				PlayerChips += CurrentBet; // Return bet
			}
			else
			{
				EndGame( "Blackjack! You win!" );
				PlayerChips += (int)(CurrentBet * 2.5f); // Blackjack pays 3:2
			}
		}
	}

	public void Hit()
	{
		if ( MiniGameState != GameState.PlayerTurn )
			return;

		PlayerHand.AddCard( Deck.DrawCard() );

		if ( PlayerHand.IsBusted() )
		{
			EndGame( "Bust! You lose!" );
		}
		else if ( PlayerHand.GetValue() == 21 )
		{
			Stand(); // Automatically stand on 21
		}
	}

	public void Stand()
	{
		if ( MiniGameState != GameState.PlayerTurn )
			return;

		MiniGameState = GameState.DealerTurn;

		// Dealer hits until 17 or higher
		while ( DealerHand.GetValue() < 17 )
		{
			DealerHand.AddCard( Deck.DrawCard() );
		}

		// Determine winner
		int playerValue = PlayerHand.GetValue();
		int dealerValue = DealerHand.GetValue();

		if ( DealerHand.IsBusted() )
		{
			EndGame( "Dealer busts! You win!" );
			PlayerChips += CurrentBet * 2;
		}
		else if ( playerValue > dealerValue )
		{
			EndGame( "You win!" );
			PlayerChips += CurrentBet * 2;
		}
		else if ( playerValue < dealerValue )
		{
			EndGame( "Dealer wins!" );
		}
		else
		{
			EndGame( "Push!" );
			PlayerChips += CurrentBet; // Return bet
		}
	}

	private void EndGame( string message )
	{
		//DealerHand.RevealCards();

		DealerHand.Cards[1].IsHidden = false; // Reveal dealer's face-down card

		MiniGameState = GameState.GameOver;
		CurrentBet = 0;
	}

	public void NewGame()
	{
		PlayerHand.Clear();
		DealerHand.Clear();
		CurrentBet = 0;
		MiniGameState = GameState.WaitingForBet;
		Deck.Shuffle();
	}

	
	private void UpdateUI( Sandbox.UI.Panel panel = null )
	{
		if ( WorldPanel.GetPanel() == null )
			return;

		Log.Info( "Updated UI Called" );
		
		/*
		var root = WorldPanel.GetPanel();

		// Update chips and bet
		var chipsLabel = root.GetChild( "chips-label" ) as Label;
		var betLabel = root.GetChild( "bet-label" ) as Label;
		
		chipsLabel.Text = $"Chips: ${PlayerChips}";
		betLabel.Text = $"Current Bet: ${CurrentBet}";

		// Update cards
		var dealerCards = root.GetChild( "dealer-cards" ) as Label;
		var dealerValue = root.GetChild( "dealer-value" ) as Label;
		var playerCards = root.GetChild( "player-cards" ) as Label;
		var playerValue = root.GetChild( "player-value" ) as Label;

		if ( gameState == GameState.PlayerTurn || gameState == GameState.DealerTurn || gameState == GameState.GameOver )
		{
			string dealerDisplay = dealerHand.GetDisplayString();
			if ( gameState == GameState.PlayerTurn && dealerHand.Cards.Count > 1 )
			{
				// Hide dealer's second card during player turn
				dealerDisplay = dealerHand.Cards[0].GetDisplayName() + ", [Hidden]";
				dealerValue?.SetText( $"Value: {dealerHand.Cards[0].GetValue()}+" );
			}
			else
			{
				dealerValue?.SetText( $"Value: {dealerHand.GetValue()}" );
			}

			dealerCards?.SetText( $"Cards: {dealerDisplay}" );
			playerCards?.SetText( $"Cards: {playerHand.GetDisplayString()}" );
			playerValue?.SetText( $"Value: {playerHand.GetValue()}" );
		}

		// Update button visibility
		var betPanel = root.GetChild( "bet-panel" );
		var gamePanel = root.GetChild( "game-panel" );

		betPanel.Style.Display = gameState == GameState.WaitingForBet ? DisplayMode.Flex : DisplayMode.None;
		gamePanel.Style.Display = gameState == GameState.PlayerTurn ? DisplayMode.Flex : DisplayMode.None;

		// Update status
		var statusLabel = root.GetChild( "status-label" ) as Label;
		string status = gameState switch
		{
			GameState.WaitingForBet => currentBet > 0 ? "Click Deal to start the game!" : "Place your bet to start!",
			GameState.PlayerTurn => "Hit or Stand?",
			GameState.DealerTurn => "Dealer's turn...",
			GameState.GameOver => GetGameResult(),
			_ => ""
		};
		statusLabel?.SetText( status );

		*/
	}
	
	public string GetStatusText()
	{
		string status = MiniGameState switch
		{
			GameState.WaitingForBet => CurrentBet > 0 ? "Click Deal to start the game!" : "Place your bet to start!",
			GameState.PlayerTurn => "Hit or Stand?",
			GameState.DealerTurn => "Dealer's turn...",
			GameState.GameOver => GetGameResult(),
			_ => ""
		};

		return status;
	}

	private string GetGameResult()
	{
		if ( PlayerHand.IsBusted() )
			return "Bust! You lose!";
		if ( DealerHand.IsBusted() )
			return "Dealer busts! You win!";
		if ( PlayerHand.IsBlackjack() && !DealerHand.IsBlackjack() )
			return "Blackjack! You win!";
		if ( !PlayerHand.IsBlackjack() && DealerHand.IsBlackjack() )
			return "Dealer has blackjack! You lose!";
		if ( PlayerHand.IsBlackjack() && DealerHand.IsBlackjack() )
			return "Push! Both have blackjack!";

		int playerValue = PlayerHand.GetValue();
		int dealerValue = DealerHand.GetValue();

		if ( playerValue > dealerValue )
			return "You win!";
		else if ( playerValue < dealerValue )
			return "Dealer wins!";
		else
			return "Push!";
	}
}


