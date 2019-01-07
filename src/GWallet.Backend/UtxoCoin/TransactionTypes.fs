namespace GWallet.Backend.UtxoCoin

open GWallet.Backend

type OutputInfo =
    {
        Amount: TransferAmount;

        // FIXME: these 2 fields below are already in the UnsignedTransactionProposal type?, we can remove it from here:
        DestinationAddress: string;
        ChangeAddress: string;
    }

type TransactionOutpointInfo =
    {
        TransactionHash: string;
        OutputIndex: int;
        ValueInSatoshis: int64;
        DestinationInHex: string;
    }

type TransactionDraft =
    {
        Inputs: List<TransactionOutpointInfo>;
        Output: OutputInfo;
    }

type TransactionMetadata =
    {
        Fee: MinerFee;
        TransactionDraft: TransactionDraft;
    }
    interface IBlockchainFeeInfo with
        member this.FeeEstimationTime with get() = this.Fee.EstimationTime
        member this.FeeValue
            with get() =
                this.Fee.EstimatedFeeInSatoshis |> UnitConversion.FromSatoshiToBtc
        member this.Currency with get() = this.Fee.Currency

