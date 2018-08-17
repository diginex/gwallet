namespace GWallet.Frontend.XF

open System
open System.Linq
open System.Threading.Tasks

open Plugin.Connectivity
open Xamarin.Forms
open Xamarin.Forms.Xaml
open ZXing.Net.Mobile.Forms

open GWallet.Backend

type TransactionInfo =
    { Metadata: IBlockchainFeeInfo;
      Destination: string; 
      Amount: TransferAmount;
    }

type SendPage(account: IAccount, receivePage: Page, newReceivePageFunc: unit->Page) as this =
    inherit ContentPage()
    let _ = base.LoadFromXaml(typeof<SendPage>)

    let GetCachedBalance() =
        // FIXME: should make sure to get the unconfirmed balance
        Caching.Instance.RetreiveLastCompoundBalance account.PublicAddress account.Currency

    let usdRateAtPageCreation = FiatValueEstimation.UsdValue account.Currency
    let cachedBalanceAtPageCreation = GetCachedBalance()

    let mainLayout = base.FindByName<StackLayout>("mainLayout")
    let destinationScanQrCodeButton = mainLayout.FindByName<Button> "destinationScanQrCodeButton"
    let transactionScanQrCodeButton = mainLayout.FindByName<Button> "transactionScanQrCodeButton"
    let currencySelectorPicker = mainLayout.FindByName<Picker>("currencySelector")
    let proposalLabel = mainLayout.FindByName<Picker> "proposalLabel"
    let transactionProposalLayout = mainLayout.FindByName<StackLayout> "transactionProposalLayout"
    let transactionProposalEntry = mainLayout.FindByName<Entry> "transactionProposalEntry"
    let amountToSend = mainLayout.FindByName<Entry> "amountToSend"
    let destinationAddressEntry = mainLayout.FindByName<Entry> "destinationAddressEntry"
    let allBalanceButton = mainLayout.FindByName<Button> "allBalance"
    let passwordEntry = mainLayout.FindByName<Entry> "passwordEntry"
    let passwordLabel = mainLayout.FindByName<Label> "passwordLabel"
    do
        let accountCurrency = account.Currency.ToString()
        currencySelectorPicker.Items.Add "USD"
        currencySelectorPicker.Items.Add accountCurrency
        currencySelectorPicker.SelectedItem <- accountCurrency
        match usdRateAtPageCreation with
        | NotFresh NotAvailable ->
            currencySelectorPicker.IsEnabled <- false
        | _ -> ()

        if Device.RuntimePlatform = Device.Android || Device.RuntimePlatform = Device.iOS then
            destinationScanQrCodeButton.IsVisible <- true

        match account with
        | :? ReadOnlyAccount ->
            Device.BeginInvokeOnMainThread(fun _ ->
                passwordEntry.IsVisible <- false
                passwordLabel.IsVisible <- false
            )
        | _ ->
            if not CrossConnectivity.IsSupported then
                failwith "cross connectivity plugin not supported for this platform?"

            use crossConnectivityInstance = CrossConnectivity.Current
            if not crossConnectivityInstance.IsConnected then
                transactionProposalLayout.IsVisible <- true
                transactionProposalEntry.IsVisible <- true
                if Device.RuntimePlatform = Device.Android || Device.RuntimePlatform = Device.iOS then
                    transactionScanQrCodeButton.IsVisible <- true
                destinationScanQrCodeButton.IsVisible <- false
                allBalanceButton.IsVisible <- false

            this.AdjustWidgetsStateAccordingToConnectivity()

    member private this.AdjustWidgetsStateAccordingToConnectivity() =
        use crossConnectivityInstance = CrossConnectivity.Current
        if not crossConnectivityInstance.IsConnected then
            Device.BeginInvokeOnMainThread(fun _ ->
                currencySelectorPicker.IsEnabled <- false
                amountToSend.IsEnabled <- false
                destinationAddressEntry.IsEnabled <- false
            )

    member this.OnTransactionScanQrCodeButtonClicked(sender: Object, args: EventArgs): unit =
        let mainLayout = base.FindByName<StackLayout> "mainLayout"

        let transactionProposalEntry = mainLayout.FindByName<Entry> "transactionProposalEntry"
        let equivalentAmount = mainLayout.FindByName<Label> "equivalentAmountInAlternativeCurrency"
        Device.BeginInvokeOnMainThread(fun _ ->
            equivalentAmount.Text <- "gonna do it 1"
        )
        let scanPage = ZXingScannerPage()
        scanPage.add_OnScanResult(fun (result:ZXing.Result) ->
            //result.
            Device.BeginInvokeOnMainThread(fun _ ->
                equivalentAmount.Text <- "gonna do it 2"
            )
            scanPage.IsScanning <- false
            Device.BeginInvokeOnMainThread(fun _ ->
                equivalentAmount.Text <- "gonna do it 3"
            )
            Device.BeginInvokeOnMainThread(fun _ ->
                equivalentAmount.Text <- "gonna do it 4"
                let task = this.Navigation.PopModalAsync()
                equivalentAmount.Text <- "gonna do it 5"
                task.ContinueWith(fun (t: Task<Page>) ->
                    Device.BeginInvokeOnMainThread(fun _ ->
                        equivalentAmount.Text <- "gonna do it 6"
                    )
                    Device.BeginInvokeOnMainThread(fun _ ->
                        passwordLabel.Text <- ("length of result is " + (result.Text.Length.ToString()))
                        transactionProposalEntry.Text <- result.Text
                        passwordLabel.Text <- (passwordLabel.Text + ("(after:"+transactionProposalEntry.Text.Length.ToString()))
                    )

                ) |> FrontendHelpers.DoubleCheckCompletionNonGeneric
            )
        )
        this.Navigation.PushModalAsync scanPage
            |> FrontendHelpers.DoubleCheckCompletionNonGeneric

    member this.OnScanQrCodeButtonClicked(sender: Object, args: EventArgs): unit =

        let mainLayout = base.FindByName<StackLayout>("mainLayout")
        let equivalentAmount = mainLayout.FindByName<Label>("equivalentAmountInAlternativeCurrency")
        Device.BeginInvokeOnMainThread(fun _ ->
            equivalentAmount.Text <- "gonna do it 1"
        )
        Threading.Thread.Sleep(TimeSpan.FromSeconds(2.0))
        let scanPage = ZXingScannerPage()
        scanPage.add_OnScanResult(fun result ->
            Device.BeginInvokeOnMainThread(fun _ ->
                equivalentAmount.Text <- "gonna do it 2"
            )
            Threading.Thread.Sleep(TimeSpan.FromSeconds(2.0))
            scanPage.IsScanning <- false

            Device.BeginInvokeOnMainThread(fun _ ->
                Device.BeginInvokeOnMainThread(fun _ ->
                    equivalentAmount.Text <- "gonna do it 3"
                )
                Threading.Thread.Sleep(TimeSpan.FromSeconds(2.0))
                let task = this.Navigation.PopModalAsync()
                Device.BeginInvokeOnMainThread(fun _ ->
                    equivalentAmount.Text <- "gonna do it 4"
                )
                Threading.Thread.Sleep(TimeSpan.FromSeconds(2.0))
                task.ContinueWith(fun (t: Task<Page>) ->
                    Device.BeginInvokeOnMainThread(fun _ ->
                        equivalentAmount.Text <- "gonna do it 5"
                    )
                    Threading.Thread.Sleep(TimeSpan.FromSeconds(2.0))
                    let address,maybeAmount =
                        match account.Currency with
                        | Currency.BTC -> UtxoCoin.Account.ParseAddressOrUrl result.Text
                        | _ -> result.Text,None

                    Device.BeginInvokeOnMainThread(fun _ ->
                        destinationAddressEntry.Text <- address
                    )
                    match maybeAmount with
                    | None -> ()
                    | Some amount ->
                        let amountLabel = mainLayout.FindByName<Entry>("amountToSend")
                        Device.BeginInvokeOnMainThread(fun _ ->
                            let cryptoCurrencyInPicker =
                                currencySelectorPicker.Items.FirstOrDefault(
                                    fun item -> item.ToString() = account.Currency.ToString()
                                )
                            if (cryptoCurrencyInPicker = null) then
                                failwithf "Could not find currency %A in picker?" account.Currency
                            currencySelectorPicker.SelectedItem <- cryptoCurrencyInPicker
                            let aPreviousAmountWasSet = not (String.IsNullOrWhiteSpace amountLabel.Text)
                            amountLabel.Text <- amount.ToString()
                            if aPreviousAmountWasSet then
                                this.DisplayAlert("Alert", "Note: new amount has been set", "OK") |> ignore
                        )
                ) |> FrontendHelpers.DoubleCheckCompletionNonGeneric
            )
        )
        this.Navigation.PushModalAsync scanPage
            |> FrontendHelpers.DoubleCheckCompletionNonGeneric

    member this.OnAllBalanceButtonClicked(sender: Object, args: EventArgs): unit =
        match cachedBalanceAtPageCreation with
        | Cached(cachedBalance,_) ->
            let usdRate = FiatValueEstimation.UsdValue account.Currency
            let mainLayout = base.FindByName<StackLayout>("mainLayout")
            let amountToSend = mainLayout.FindByName<Entry>("amountToSend")

            let allBalanceAmount =
                match currencySelectorPicker.SelectedItem.ToString() with
                | "USD" ->
                    match usdRate with
                    | Fresh rate | NotFresh(Cached(rate,_)) ->
                        cachedBalance * rate
                    | NotFresh NotAvailable ->
                        failwith "if no usdRate was available, currencySelectorPicker should have been disabled, so it shouldn't have 'USD' selected"
                | _ -> cachedBalance
            amountToSend.Text <- allBalanceAmount.ToString()
        | _ ->
            failwith "if no balance was available(offline?), allBalance button should have been disabled"

    member this.OnCurrencySelectorTextChanged(sender: Object, args: EventArgs): unit =

        let currentAmountTypedEntry = mainLayout.FindByName<Entry>("amountToSend")
        let currentAmountTyped = currentAmountTypedEntry.Text
        match Decimal.TryParse currentAmountTyped with
        | false,_ ->
            ()
        | true,decimalAmountTyped ->
            let usdRate = FiatValueEstimation.UsdValue account.Currency
            match usdRate with
            | NotFresh NotAvailable ->
                failwith "if no usdRate was available, currencySelectorPicker should have been disabled, so it shouldn't have 'USD' selected"
            | Fresh rate | NotFresh(Cached(rate,_)) ->

                //FIXME: maybe only use ShowDecimalForHumans if amount in textbox is not allbalance?
                let convertedAmount =
                    // we choose the WithMax overload because we don't want to surpass current allBalance & be red
                    match currencySelectorPicker.SelectedItem.ToString() with
                    | "USD" ->
                        match cachedBalanceAtPageCreation with
                        | Cached(cachedBalance,_) ->
                            FrontendHelpers.ShowDecimalForHumansWithMax CurrencyType.Fiat
                                                                        (rate * decimalAmountTyped)
                                                                        (cachedBalance * rate)
                        | _ ->
                            failwith "if no balance was available(offline?), currencySelectorPicker should have been disabled, so it shouldn't have 'USD' selected"
                    | _ ->
                        Formatting.DecimalAmount CurrencyType.Crypto (decimalAmountTyped / rate)
                currentAmountTypedEntry.Text <- convertedAmount

    member private this.SendTransaction (account: NormalAccount) (transactionInfo: TransactionInfo) (password: string) =
        let maybeTxId =
            try
                Account.SendPayment account
                                    transactionInfo.Metadata
                                    transactionInfo.Destination
                                    transactionInfo.Amount
                                    password
                                        |> Async.RunSynchronously |> Some
            with
            | :? DestinationEqualToOrigin ->
                let errMsg = "Transaction's origin cannot be the same as the destination."
                Device.BeginInvokeOnMainThread(fun _ ->
                    this.DisplayAlert("Alert", errMsg, "OK").ContinueWith(fun _ ->
                        this.ToggleInputWidgetsEnabledOrDisabled true
                    ) |> FrontendHelpers.DoubleCheckCompletionNonGeneric
                )
                None
            | :? InsufficientFunds ->
                let errMsg = "Insufficient funds."
                Device.BeginInvokeOnMainThread(fun _ ->
                    this.DisplayAlert("Alert", errMsg, "OK").ContinueWith(fun _ ->
                        this.ToggleInputWidgetsEnabledOrDisabled true
                    ) |> FrontendHelpers.DoubleCheckCompletionNonGeneric
                )
                None
            | :? InvalidPassword ->
                let errMsg = "Invalid password, try again."
                Device.BeginInvokeOnMainThread(fun _ ->
                    this.DisplayAlert("Alert", errMsg, "OK").ContinueWith(fun _ ->
                        this.ToggleInputWidgetsEnabledOrDisabled true
                    ) |> FrontendHelpers.DoubleCheckCompletionNonGeneric
                )
                None
        
        match maybeTxId with
        | None -> ()
        | Some txIdUrlInBlockExplorer ->
            // TODO: allow linking to tx in a button or something?
            Device.BeginInvokeOnMainThread(fun _ ->
                this.DisplayAlert("Success", "Transaction sent.", "OK")
                    .ContinueWith(fun _ ->
                        Device.BeginInvokeOnMainThread(fun _ ->
                            let newReceivePage = newReceivePageFunc()
                            let navNewReceivePage = NavigationPage(newReceivePage)
                            NavigationPage.SetHasNavigationBar(newReceivePage, false)
                            NavigationPage.SetHasNavigationBar(navNewReceivePage, false)
                            receivePage.Navigation.RemovePage receivePage
                            this.Navigation.InsertPageBefore(navNewReceivePage, this)

                            this.Navigation.PopAsync() |> FrontendHelpers.DoubleCheckCompletion
                        )
                    ) |> FrontendHelpers.DoubleCheckCompletionNonGeneric
            )
    
    member private this.ValidateAddress currency destinationAddress =
        let inputAddress = destinationAddress
        try
            Account.ValidateAddress currency destinationAddress
            Some(destinationAddress)
        with
        | AddressMissingProperPrefix(possiblePrefixes) ->
            let possiblePrefixesStr = String.Join(", ", possiblePrefixes)
            let msg =  (sprintf "Address starts with the wrong prefix. Valid prefixes: %s."
                                    possiblePrefixesStr)
            this.DisplayAlert("Alert", msg, "OK") |> ignore
            None
        | AddressWithInvalidLength(lengthLimitViolated) ->
            let msg =
                if (inputAddress.Length > lengthLimitViolated) then
                    (sprintf "Address should have a length not higher than %d characters, please try again."
                        lengthLimitViolated)
                else if (inputAddress.Length < lengthLimitViolated) then
                    (sprintf "Address should have a length not lower than %d characters, please try again."
                        lengthLimitViolated)
                else
                    failwith (sprintf "Address introduced '%s' gave a length error with a limit that matches its length: %d=%d"
                                 inputAddress lengthLimitViolated inputAddress.Length)
            this.DisplayAlert("Alert", msg, "OK") |> ignore
            None
        | AddressWithInvalidChecksum(maybeAddressWithValidChecksum) ->
            let final =
                match maybeAddressWithValidChecksum with
                | None -> None
                | _ ->
                    //FIXME: warn user about bad checksum in any case (not only if the original address has mixed
                    // lowecase and uppercase like if had been validated, to see if he wants to continue or not
                    // (this text is better borrowed from the Frontend.Console project)
                    if not (destinationAddress.All(fun char -> Char.IsLower char)) then
                        None
                    else
                        maybeAddressWithValidChecksum
            if final.IsNone then
                let msg = "Address doesn't seem to be valid, please try again."
                this.DisplayAlert("Alert", msg, "OK") |> ignore
            final

    member private this.IsPasswordUnfilledAndNeeded (mainLayout: StackLayout) =
        match account with
        | :? ReadOnlyAccount ->
            false
        | _ ->
            if passwordEntry = null then
                // not ready yet?
                true
            else
                String.IsNullOrEmpty passwordEntry.Text

    member this.OnTransactionProposalEntryTextChanged (sender: Object, args: EventArgs): unit =

        let mainLayout = base.FindByName<StackLayout> "mainLayout"
        let equivalentAmount = mainLayout.FindByName<Label>("equivalentAmountInAlternativeCurrency")
        Device.BeginInvokeOnMainThread(fun _ ->
            equivalentAmount.Text <- "gonna do it 7"
        )
        let transactionProposalEntry = mainLayout.FindByName<Entry> "transactionProposalEntry"
        let transactionProposalEntryText = transactionProposalEntry.Text
        if not (String.IsNullOrWhiteSpace transactionProposalEntryText) then
            Device.BeginInvokeOnMainThread(fun _ ->
                equivalentAmount.Text <- "gonna do it 8"
            )
            let maybeUnsignedTransaction =
                try
                    Account.ImportUnsignedTransactionFromJson transactionProposalEntryText |> Some
                with
                | :? DeserializationException as dex ->
                    Device.BeginInvokeOnMainThread(fun _ ->
                        transactionProposalEntry.TextColor <- Color.Red
                        let errMsg = "Transaction proposal corrupt or invalid"
                        this.DisplayAlert("Alert", errMsg, "OK") |> FrontendHelpers.DoubleCheckCompletionNonGeneric
                    )
                    None
            match maybeUnsignedTransaction with
            | None -> ()
            | Some unsignedTransaction ->
                Device.BeginInvokeOnMainThread(fun _ ->
                    equivalentAmount.Text <- "gonna do it 9"
                )
                if account.PublicAddress <> unsignedTransaction.Proposal.OriginAddress then
                    Device.BeginInvokeOnMainThread(fun _ ->
                        transactionProposalEntry.TextColor <- Color.Red
                        let errMsg = "Transaction proposal's sender address doesn't match with this currency's account"
                        this.DisplayAlert("Alert", errMsg, "OK") |> FrontendHelpers.DoubleCheckCompletionNonGeneric
                    )
                else
                    Device.BeginInvokeOnMainThread(fun _ ->
                        equivalentAmount.Text <- "gonna do it 10"
                    )
                    // to locally save balances and fiat rates from the online device
                    //Caching.Instance.SaveSnapshot unsignedTransaction.Cache
                    Device.BeginInvokeOnMainThread(fun _ ->
                        equivalentAmount.Text <- "gonna do it 11"
                    )
                    Device.BeginInvokeOnMainThread(fun _ ->
                        destinationAddressEntry.Text <- unsignedTransaction.Proposal.DestinationAddress
                        amountToSend.Text <- unsignedTransaction.Proposal.Amount.ValueToSend.ToString()
                        passwordEntry.Focus() |> ignore
                    )
        ()

    member this.OnEntryTextChanged(sender: Object, args: EventArgs) =
        Console.WriteLine("____________________OnEntryTextChanged 1")
        let usdRate = FiatValueEstimation.UsdValue account.Currency
        let mainLayout = base.FindByName<StackLayout>("mainLayout")
        if (mainLayout = null) then
            //page not yet ready
            ()
        else
            Console.WriteLine("____________________OnEntryTextChanged 2")
            let amountToSend = mainLayout.FindByName<Entry>("amountToSend")
            if (destinationAddressEntry = null ||
                String.IsNullOrEmpty destinationAddressEntry.Text ||
                this.IsPasswordUnfilledAndNeeded mainLayout ||
                amountToSend = null) then
                ()
            else
                Console.WriteLine("____________________OnEntryTextChanged 3")
                let equivalentAmount = mainLayout.FindByName<Label>("equivalentAmountInAlternativeCurrency")
                let sendButton = mainLayout.FindByName<Button>("sendButton")
                if (amountToSend.Text <> null && amountToSend.Text.Length > 0) then

                    Console.WriteLine("____________________OnEntryTextChanged 4")
                    // FIXME: marking as red should not even mark button as disabled but give the reason in Alert?
                    match Decimal.TryParse(amountToSend.Text) with
                    | false,_ ->
                        Console.WriteLine("____________________OnEntryTextChanged 5")
                        amountToSend.TextColor <- Color.Red
                        sendButton.IsEnabled <- false
                        equivalentAmount.Text <- String.Empty
                    | true,amount ->
                        Console.WriteLine("____________________OnEntryTextChanged 6")
                        let lastCachedBalance: decimal =
                            match GetCachedBalance() with
                            | Cached(lastCachedBalance,_) ->
                                lastCachedBalance
                            | _ ->
                                failwith "there should be a cached balance (either by being online, or because of importing a cache snapshot) at the point of changing the amount or destination address (respectively, by the user, or by importing a tx proposal)"

                        Console.WriteLine("____________________OnEntryTextChanged 7")
                        let allBalanceInSelectedCurrency =
                            match currencySelectorPicker.SelectedItem.ToString() with
                            | "USD" ->
                                match usdRate with
                                | NotFresh NotAvailable ->
                                    failwith "if no usdRate was available, currencySelectorPicker should have been disabled, so it shouldn't have 'USD' selected"
                                | NotFresh(Cached(rate,_)) | Fresh rate ->
                                    lastCachedBalance * rate
                            | _ -> lastCachedBalance

                        Console.WriteLine("____________________OnEntryTextChanged 8")
                        if (amount <= 0.0m || amount > allBalanceInSelectedCurrency) then
                            Console.WriteLine("____________________OnEntryTextChanged 9")
                            amountToSend.TextColor <- Color.Red
                            sendButton.IsEnabled <- false
                            equivalentAmount.Text <- "(Not enough funds)"
                        else
                            Console.WriteLine("____________________OnEntryTextChanged 10")
                            amountToSend.TextColor <- Color.Default
                            sendButton.IsEnabled <- (not (this.IsPasswordUnfilledAndNeeded mainLayout)) &&
                                                    destinationAddressEntry.Text <> null &&
                                                    destinationAddressEntry.Text.Length > 0

                            match usdRate with
                            | NotFresh NotAvailable -> ()
                            | NotFresh(Cached(rate,_)) | Fresh rate ->
                                Console.WriteLine("____________________OnEntryTextChanged 11")
                                let eqAmount,otherCurrency =
                                    match currencySelectorPicker.SelectedItem.ToString() with
                                    | "USD" ->
                                        Formatting.DecimalAmount CurrencyType.Crypto (amount / rate),
                                            account.Currency.ToString()
                                    | _ ->
                                        Formatting.DecimalAmount CurrencyType.Fiat (rate * amount),
                                            "USD"
                                Console.WriteLine("____________________OnEntryTextChanged 12")
                                let usdAmount = sprintf "~ %s %s" eqAmount otherCurrency
                                equivalentAmount.Text <- usdAmount
                else
                    Console.WriteLine("____________________OnEntryTextChanged 13")
                    sendButton.IsEnabled <- false

    member this.OnCancelButtonClicked(sender: Object, args: EventArgs) =
        Device.BeginInvokeOnMainThread(fun _ ->
            receivePage.Navigation.PopAsync() |> FrontendHelpers.DoubleCheckCompletion
        )

    member private this.ToggleInputWidgetsEnabledOrDisabled (enabled: bool) =
        let sendButton = mainLayout.FindByName<Button> "sendButton"
        let cancelButton = mainLayout.FindByName<Button> "cancelButton"
        let transactionScanQrCodeButton = mainLayout.FindByName<Button> "transactionScanQrCodeButton"
        let destinationScanQrCodeButton = mainLayout.FindByName<Button> "destinationScanQrCodeButton"
        let destinationAddressEntry = mainLayout.FindByName<Entry> "destinationAddressEntry"
        let allBalanceButton = mainLayout.FindByName<Button> "allBalance"
        let currencySelectorPicker = mainLayout.FindByName<Picker> "currencySelector"
        let amountToSendEntry = mainLayout.FindByName<Entry> "amountToSend"
        let passwordEntry = mainLayout.FindByName<Entry> "passwordEntry"

        let newSendButtonCaption =
            if enabled then
                "Send"
            else
                "Sending..."

        Device.BeginInvokeOnMainThread(fun _ ->
            sendButton.IsEnabled <- enabled
            cancelButton.IsEnabled <- enabled
            transactionScanQrCodeButton.IsEnabled <- enabled
            destinationScanQrCodeButton.IsEnabled <- enabled
            destinationAddressEntry.IsEnabled <- enabled
            allBalanceButton.IsEnabled <- enabled
            currencySelectorPicker.IsEnabled <- enabled
            amountToSendEntry.IsEnabled <- enabled
            passwordEntry.IsEnabled <- enabled
            sendButton.Text <- newSendButtonCaption
        )

        this.AdjustWidgetsStateAccordingToConnectivity()

    member private this.AnswerToFee (account: IAccount) (txInfo: TransactionInfo) (answer: Task<bool>):unit =
        if (answer.Result) then
            match account with
            | :? NormalAccount as normalAccount ->
                let passwordEntry = mainLayout.FindByName<Entry> "passwordEntry"
                let password = passwordEntry.Text
                Task.Run(fun _ -> this.SendTransaction normalAccount txInfo password)
                    |> FrontendHelpers.DoubleCheckCompletionNonGeneric
            | :? ReadOnlyAccount as readOnlyAccount ->
                let proposal = {
                    OriginAddress = account.PublicAddress;
                    Amount = txInfo.Amount;
                    DestinationAddress = txInfo.Destination;
                }
                let compressedTxProposal = Account.SerializeUnsignedTransaction proposal txInfo.Metadata true

                let coldMsg =
                    "Account belongs to cold storage, so you'll need to scan this as a transaction proposal in the next page."
                Device.BeginInvokeOnMainThread(fun _ ->
                    let alertColdStorageTask =
                        this.DisplayAlert("Alert",
                                          coldMsg,
                                          "OK")
                    alertColdStorageTask.ContinueWith(
                        fun _ ->
                            let pairTransactionProposalPage = PairingFromPage(this,
                                                                              "Copy proposal to the clipboard",
                                                                              compressedTxProposal)
                            NavigationPage.SetHasNavigationBar(pairTransactionProposalPage, false)
                            let navPairPage = NavigationPage pairTransactionProposalPage
                            NavigationPage.SetHasNavigationBar(navPairPage, false)
                            Device.BeginInvokeOnMainThread(fun _ ->
                                this.Navigation.PushAsync navPairPage
                                    |> FrontendHelpers.DoubleCheckCompletionNonGeneric
                            )

                    ) |> FrontendHelpers.DoubleCheckCompletionNonGeneric
                )

            | _ ->
                failwith "Unexpected SendPage instance running on weird account type"
        else
            this.ToggleInputWidgetsEnabledOrDisabled true

    member private this.ShowFeeAndSend (maybeTxMetadataWithFeeEstimation: Option<IBlockchainFeeInfo>,
                                        transferAmount: TransferAmount,
                                        destinationAddress: string) =
        match maybeTxMetadataWithFeeEstimation with
        | None -> ()
        | Some txMetadataWithFeeEstimation ->
            let feeCurrency = txMetadataWithFeeEstimation.Currency
            let usdRateForCurrency = FiatValueEstimation.UsdValue feeCurrency
            match usdRateForCurrency with
            | NotFresh _ ->
                // this probably would never happen, because without internet connection we may get
                // then txFeeInfoTask throw before... so that's why I write the TODO below...
                Device.BeginInvokeOnMainThread(fun _ ->
                    let alertInternetConnTask =
                        this.DisplayAlert("Alert",
                                          // TODO: support cold storage mode here
                                          "Internet connection not available at the moment, try again later",
                                          "OK")
                    alertInternetConnTask.ContinueWith(fun _ -> this.ToggleInputWidgetsEnabledOrDisabled true)
                        |> FrontendHelpers.DoubleCheckCompletionNonGeneric
                )
            | Fresh someUsdValue ->

                let feeInCrypto = txMetadataWithFeeEstimation.FeeValue
                let feeInFiatValue = someUsdValue * feeInCrypto
                let feeInFiatValueStr = sprintf "~ %s USD"
                                                (Formatting.DecimalAmount CurrencyType.Fiat feeInFiatValue)

                let feeAskMsg = sprintf "Estimated fee for this transaction would be: %s %s (%s)"
                                      (Formatting.DecimalAmount CurrencyType.Crypto feeInCrypto)
                                      (txMetadataWithFeeEstimation.Currency.ToString())
                                      feeInFiatValueStr
                Device.BeginInvokeOnMainThread(fun _ ->
                    let askFeeTask = this.DisplayAlert("Alert", feeAskMsg, "OK", "Cancel")

                    let txInfo = { Metadata = txMetadataWithFeeEstimation;
                                   Amount = transferAmount;
                                   Destination = destinationAddress; }

                    askFeeTask.ContinueWith(this.AnswerToFee account txInfo) |> FrontendHelpers.DoubleCheckCompletion
                )


    member this.OnSendButtonClicked(sender: Object, args: EventArgs): unit =
        let mainLayout = base.FindByName<StackLayout>("mainLayout")
        let amountToSend = mainLayout.FindByName<Entry>("amountToSend")
        let destinationAddress = destinationAddressEntry.Text

        match Decimal.TryParse(amountToSend.Text) with
        | false,_ ->
            this.DisplayAlert("Alert", "The amount should be a decimal amount", "OK")
                |> FrontendHelpers.DoubleCheckCompletionNonGeneric
        | true,amount ->
            if not (amount > 0.0m) then
                this.DisplayAlert("Alert", "Amount should be positive", "OK")
                    |> FrontendHelpers.DoubleCheckCompletionNonGeneric
            else

                let amountInAccountCurrency =
                    match currencySelectorPicker.SelectedItem.ToString() with
                    | "USD" ->

                        // FIXME: we should probably just grab the amount from the equivalentAmountInAlternativeCurrency
                        // label to prevent rate difference from the moment the amount was written until the "Send"
                        // button was pressed
                        let usdRate = FiatValueEstimation.UsdValue account.Currency

                        match usdRate with
                        | Fresh rate | NotFresh(Cached(rate,_)) ->
                            amount / rate
                        | NotFresh NotAvailable ->
                            failwith "if no usdRate was available, currencySelectorPicker should have been disabled, so it shouldn't have 'USD' selected"
                    | _ -> amount

                let currency = account.Currency

                let transferAmount =
                    match GetCachedBalance() with
                    | Cached(lastCachedBalance,_) ->
                        TransferAmount(amountInAccountCurrency, lastCachedBalance, currency)
                    | _ ->
                        failwith "there should be a cached balance (either by being online, or because of importing a cache snapshot) at the point of clicking the send button"

                this.ToggleInputWidgetsEnabledOrDisabled false

                let maybeValidatedAddress = this.ValidateAddress currency destinationAddress
                match maybeValidatedAddress with
                | None -> this.ToggleInputWidgetsEnabledOrDisabled true
                | Some validatedDestinationAddress ->

                    let maybeTxMetadataWithFeeEstimationAsync = async {
                        try
                            let! txMetadataWithFeeEstimation =
                                Account.EstimateFee account transferAmount validatedDestinationAddress
                            return Some txMetadataWithFeeEstimation
                        with
                        | :? InsufficientBalanceForFee ->
                            Device.BeginInvokeOnMainThread(fun _ ->
                                let alertLowBalanceForFeeTask =
                                    this.DisplayAlert("Alert",
                                                      // TODO: support cold storage mode here
                                                      "Remaining balance would be too low for the estimated fee, try sending lower amount",
                                                      "OK")
                                alertLowBalanceForFeeTask.ContinueWith(
                                    fun _ -> this.ToggleInputWidgetsEnabledOrDisabled true
                                )
                                    |> FrontendHelpers.DoubleCheckCompletionNonGeneric
                            )
                            return None
                    }

                    let maybeTxMetadataWithFeeEstimationTask = maybeTxMetadataWithFeeEstimationAsync |> Async.StartAsTask

                    maybeTxMetadataWithFeeEstimationTask.ContinueWith(fun (txMetadataWithFeeEstimationTask: Task<Option<IBlockchainFeeInfo>>) ->
                        this.ShowFeeAndSend(txMetadataWithFeeEstimationTask.Result,
                                            transferAmount,
                                            validatedDestinationAddress)
                    ) |> FrontendHelpers.DoubleCheckCompletionNonGeneric

                    ()
