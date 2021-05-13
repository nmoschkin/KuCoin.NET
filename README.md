# Kucoin.NET
KuCoin API Libraries written in .NET 5.

This library is in active development, and it's brand new so things will undoubtedly change while I consider next steps.

As of May 13th, 2021, I would guage that the library, itself, is about 80-90% complete. 

## REST API 

Inside the Kucoin.NET.Rest namespace you will find three objects, __Market__, __Order__, and __User__.  __User__ and __Order__ require API Keys.  

There are two credentials providers, there is the default __MemoryEncryptedCredentialsProvider__ that is in the Kucoin.NET library, and then the __CryptoCredentials__ class in the example app.

The __MemoryEncryptedCredentialsProvider__ will store the credentials encrypted in memory with a random seed, until they are needed.  __CryptoCredentials__ has the ability to load and save encrypted credentials sets to disk.  A 6 digit numeric pin is required to save and load credentials.  Both of these classes implement the __ICredentialsProvider__ interface which you can use to write your own provider.

## Websocket Feeds

Public feeds in the namespace __Kucoin.NET.Websockets.Public__:

  - __AllTickerFeed__ - Pushes all symbol tickers as they are updated.
  - __KlineFeed__ - Pushes the K-Line feed for the subscribed symbols.
  - __MarketFeed__ - Pushes an entire market.
  - __SnapshotFeed__ - Pushes market snapshots.
  - __TickerFeed__ - Pushes basic symbol price tickers.

Private feeds in the namespace __Kucoin.NET.Websockets.Private__:

  - __Level2__ - Pushes the full-depth Level 2 market feed (calibrated).
  - __Level2Depth5/Level2Depth50__ - Pushes the 5/50 best ask/bid static market depth feeds.
  - __Level3__ - Level 3 Full Match Engine __(IN PROGRESS)__

All of the feeds support multiplexing.  You may create a single feed object, and use that object's connection to start sub-channels that will be served to the multiplex child classes.  Multiplexing is implemented in the __KucoinBaseWebsocketFeed__ abstract class.  
  
  * Note: You cannot multiplex a private feed onto a public feed.

Granular observations are possible with feeds that support more than one symbol subscribed simultaneously.  These are based on the __GranularFeedBase__ abstract class.  

All of the feeds implement the __IObservable<T>__ pattern.  The ViewModels in the example app implement the __IObserver<T>__ pattern where applicable.

The library, itself, is pretty well documented, so far.  You can find plenty of usage examples in the sample app, in __Program.cs__, __MainWindowViewModel.cs__, and __AccountsWindowViewModel.cs__.

The library is being implemented as described in (https://docs.kucoin.com/#general).

The example project uses my fork of the __FancyCandles__ chart library. It is included via a git submodule.

When you clone the project, be sure to run __git submodule init__ and __git submodule update__ from the root project directory to download and sync that project.

This project is epic, and there are bound to be bugs.  Feel free to open issues, and I will get to them, as I can, if I don't find the bugs, first.  I will post more updates here when the project nears completion and I plan to include a library for __KuCoin Futures__, after this one is feature complete.  


