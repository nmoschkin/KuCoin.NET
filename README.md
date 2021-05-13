# Kucoin.NET
KuCoin API Libraries written in .NET Standard 2.0

These are __not__ official libraries from KuCoin.  They are new implementations.  KuCoin does not currently offer an open-source C# library for .NET, to my knowledge.

This library is in active development, and it's brand new so things will undoubtedly change while I consider next steps.

As of May 13th, 2021, I would guage that the library, itself, is about 70-80% complete. 

## Installation

The library is being implemented as described in (https://docs.kucoin.com/#general), in .NET Standard 2.0.  It will compile anywhere the .NET Standard library is supported (Windows/Linux/Android/MacOS/iOS/etc.)

The example project is built in .NET 5 using WPF for Windows Desktop, and uses my fork of the __FancyCandles__ chart library. It is included via a git submodule.

When you clone the project, you must run __git submodule init__ and __git submodule update__ from the root project directory to download and sync the __FancyCandles__ project.

## REST API 

Inside the __Kucoin.NET.Rest__ namespace you will find three objects, __Market__, __Order__, and __User__.  __User__ and __Order__ require API Keys.  

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

_In order to use the feeds, you need to initialize the __Kucoin.NET.Helpers.Dispatcher__ static class with a __SynchronizationContext__ from the __Dispatcher__ provided by your application (usually the App class, itself.)  Feed observations will not execute correctly without a __SynchronizationContext__, because they need to inform the UI thread._

All of the feeds support multiplexing.  You may create a single feed object, and use that object's connection to start sub-channels that will be served to the multiplex child classes.  Multiplexing is implemented in the __KucoinBaseWebsocketFeed__ abstract class.  
  
  * Note: You cannot multiplex a private feed onto a public feed.

Granular observations are possible with feeds that support more than one symbol subscribed simultaneously.  These are based on the __GranularFeedBase__ abstract class.  

All of the feeds implement the __IObservable<T>__ pattern.  The ViewModels in the example app implement the __IObserver<T>__ pattern where applicable.

## Running the sample app

The example application will ask you for a 6 digit pin when you first start it.  Choose any pin you want, but you need to remember it to access your credentials.

![](docs/Sample2.png?raw=true)

Once the application starts for the first time, it will create a new random __Guid__ and store it in the registry. This Guid value, together with your pin, will be used to encrypt credentials on both disk and in memory, when not in use.  

Click the button on the bottom right-hand side that says 'Edit Credentials' to enter and save your credentials.  When you start the app the next time, enter the same pin to automatically load the credentials.

You can use a different pin to store a different set of credentials.  

## Other Notes

The library, itself, is pretty well documented, so far.  You can find plenty of usage examples in the sample app, in __Program.cs__, __MainWindowViewModel.cs__, and __AccountsWindowViewModel.cs__.

This project is epic, and there are bound to be bugs.  Feel free to open issues, and I will get to them, as I can, if I don't find the bugs, first.  I will post more updates here when the project nears completion and I plan to include a library for __KuCoin Futures__, after this one is feature complete.  

## Screenshot Of Sample App

![](docs/Sample1.png?raw=true)

## And finally, I take donations!  

If you like my work, and find it may be useful, you can donate crypto if you like!

My Ethereum wallet address is: 
  - 0xb97a29b4349cb3f66b7f2143c6ba1362b8ec4e7d

My Stellar Lumens (XLM) address is:
  - Address: GAJ4BSGJE6UQHZAZ5U5IUOABPDCYPKPS3RFS2NVNGFGFXGVQDLBQJW2P
  - Memo: 1870588215

My KCS wallet is:
  - 0xb97a29b4349cb3f66b7f2143c6ba1362b8ec4e7d

My DOGE wallet is:
  - DCnwUYnjvHQYhr1L2iNvRwxJmCyt6TZWXA
   


