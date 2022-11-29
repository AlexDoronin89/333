using ConnectionLibrary;
using ConnectionLibrary.Entity;
using ConnectionLibrary.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        private const int RowCount = 10;
        private const int ColumnCount = 10;

        private static void Main(string[] args)
        {
            Random random = new Random();
            int currentStepNumber = 0;
            CellValue[,] firstPlayerField = new CellValue[RowCount, ColumnCount];
            CellValue[,] secondPlayerField = new CellValue[RowCount, ColumnCount];
            ClearField(firstPlayerField);
            ClearField(secondPlayerField);
            firstPlayerField = RandomField(firstPlayerField, random);
            secondPlayerField = RandomField(secondPlayerField, random);

            TcpListener server = ConnectionTools.GetListener();

            GameStatus gameStatus = GameStatus.Play;

            Logger.Log("SERVER STARTED");

            TcpClient playerFirstCrossClient = AcceptClient(server, ConstantData.PlayerChars.First);
            TcpClient playerSecondClient = AcceptClient(server, ConstantData.PlayerChars.Second);

            ConnectionTools.SendResponce(playerFirstCrossClient, ConstantData.GameStates.Go);
            ConnectionTools.SendResponce(playerSecondClient, ConstantData.GameStates.Wait);

            while (gameStatus == GameStatus.Play)
            {
                currentStepNumber++;

                SetStepParameters(currentStepNumber, playerFirstCrossClient, playerSecondClient,
                    out string currentValue, out TcpClient currentPlayer,
                    out string stateCrossAfter, out string stateZeroAfter);

                bool isStepEnd = false;

                while (isStepEnd == false)
                {
                    Request request = ConnectionTools.GetRequest(currentPlayer);

                    if (request.Command == ConstantData.Commands.Step)
                        Logger.Log($"STEP №{currentStepNumber} by {currentValue}: {request.Parameters[0]},{request.Parameters[1]}");

                    switch (request.Command)
                    {
                        case ConstantData.Commands.Step:
                            if (currentValue == "1")
                                ProcessCommandStep(currentPlayer, firstPlayerField, request.Parameters, currentValue);
                            else if (currentValue == "2")
                                ProcessCommandStep(currentPlayer, secondPlayerField, request.Parameters, currentValue);
                            break;
                        case ConstantData.Commands.EndStep:
                            isStepEnd = true;
                            ProcessCommandEndStep(firstPlayerField, secondPlayerField, playerFirstCrossClient, playerSecondClient, stateCrossAfter, stateZeroAfter);
                            break;
                        case ConstantData.Commands.GetField:
                            ProcessCommandGetField(currentPlayer, firstPlayerField);
                            break;
                        case ConstantData.Commands.GetOpponentField:
                            ProcessCommandGetField(currentPlayer, secondPlayerField);
                            break;
                    }
                }

                gameStatus = GetGameStatus(firstPlayerField, secondPlayerField);
                Logger.Log("=========" + GetGameStatusString(gameStatus));
            }

            ProcessCommandGetField(playerFirstCrossClient, firstPlayerField);
            ProcessCommandGetField(playerSecondClient, secondPlayerField);

            ConnectionTools.SendResponce(playerFirstCrossClient, GetGameStatusString(gameStatus));
            ConnectionTools.SendResponce(playerSecondClient, GetGameStatusString(gameStatus));

            playerFirstCrossClient.Close();
            playerSecondClient.Close();

            server.Stop();

            Logger.Log("SERVER STOPED");
            Console.ReadLine();
        }

        private static string GetGameStatusString(GameStatus status)
        {
            switch (status)
            {
                case GameStatus.Play:
                    return ConstantData.GameStatus.Play;
                case GameStatus.WinFirst:
                    return ConstantData.GameStatus.WinFirst;
                case GameStatus.WinSecond:
                    return ConstantData.GameStatus.WinSecond;
            }

            return string.Empty;
        }

        private static void ProcessCommandEndStep(CellValue[,] firstField, CellValue[,] secondField, TcpClient playerCross, TcpClient playerZero, string stateCrossAfter, string stateZeroAfter)
        {
            ConnectionTools.SendResponce(playerCross, stateCrossAfter);
            ConnectionTools.SendResponce(playerZero, stateZeroAfter);

            bool isEndGame = GetGameStatus(firstField, secondField) == GameStatus.Play;
            string endResult = isEndGame ? ConstantData.ResponceResults.Ok : ConstantData.GameStates.End;

            ConnectionTools.SendResponce(playerCross, endResult);
            ConnectionTools.SendResponce(playerZero, endResult);
        }

        private static void ProcessCommandGetField(TcpClient player, CellValue[,] field)
        {
            ConnectionTools.SendResponce(player, GetField(field));
        }

        private static string GetField(CellValue[,] field)
        {
            string textField = string.Empty;

            for (int i = 0; i < RowCount; i++)
            {
                for (int j = 0; j < ColumnCount; j++)
                {
                    switch (field[i, j])
                    {
                        case CellValue.Empty:
                            textField += "-";
                            break;
                        case CellValue.Ship:
                            textField += ConstantData.PlayerChars.Ship;
                            break;
                        case CellValue.First:
                            textField += ConstantData.PlayerChars.First;
                            break;
                        case CellValue.FirstHit:
                            textField += ConstantData.PlayerChars.HitFirst;
                            break;
                        case CellValue.Second:
                            textField += ConstantData.PlayerChars.Second;
                            break;
                        case CellValue.SecondHit:
                            textField += ConstantData.PlayerChars.HitSecond;
                            break;
                    }
                }

                textField += ":";
            }

            return textField;
        }

        private static void ProcessCommandStep(TcpClient player, CellValue[,] field, string[] coordinates, string value)
        {
            bool stepResult;

            int.TryParse(coordinates[0], out int i);
            int.TryParse(coordinates[1], out int j);

            stepResult = TryMakeStep(field, i, j, value == ConstantData.PlayerChars.First ? CellValue.First : CellValue.Second);

            string responce = stepResult ? value : string.Empty;
            ConnectionTools.SendResponce(player, responce);
        }

        private static bool TryMakeStep(CellValue[,] field, int i, int j, CellValue value)
        {
            if ((i > RowCount || j > ColumnCount || field[i - 1, j - 1] != CellValue.Empty)
                && field[i - 1, j - 1] == CellValue.First || field[i - 1, j - 1] == CellValue.Second)
                return false;

            if (field[i - 1, j - 1] != CellValue.Ship)
                field[i - 1, j - 1] = value;
            else if (field[i - 1, j - 1] == CellValue.Ship)
                if (value == CellValue.First)
                    field[i - 1, j - 1] = CellValue.FirstHit;
                else if(value == CellValue.Second)
                    field[i - 1, j - 1] = CellValue.SecondHit;

            return true;
        }

        private static GameStatus GetGameStatus(CellValue[,] firstPlayerField, CellValue[,] secondPlayerField)
        {
            if (CheckWinCondition(firstPlayerField))
                return GameStatus.WinFirst;

            if (CheckWinCondition(secondPlayerField))
                return GameStatus.WinSecond;

            return GameStatus.Play;
        }

        private static bool CheckWinCondition(CellValue[,] field)
        {
            int shipcount = 0;

            for (int i = 0; i < field.GetLength(0); i++)
            {
                for (int j = 0; j < field.GetLength(1); j++)
                {
                    if (field[i, j] == CellValue.Ship)
                    {
                        shipcount++;
                    }
                }
            }

            if (shipcount == 0)
                return true;

            return false;
        }

        private static bool HasEmpty(CellValue[,] field)
        {
            for (int i = 0; i < RowCount; i++)
                for (int j = 0; j < ColumnCount; j++)
                    if (field[i, j] == CellValue.Empty)
                        return true;

            return false;
        }

        private static void SetStepParameters(int currentStep, TcpClient playerFirstClient, TcpClient playerSecondClient,
            out string currentValue, out TcpClient currentPlayer, out string stateFirstAfter, out string stateSecondAfter)
        {
            if (currentStep % 2 == 0)
            {
                currentValue = ConstantData.PlayerChars.Second;
                currentPlayer = playerSecondClient;
                stateFirstAfter = ConstantData.GameStates.Go;
                stateSecondAfter = ConstantData.GameStates.Wait;
            }
            else
            {
                currentValue = ConstantData.PlayerChars.First;
                currentPlayer = playerFirstClient;
                stateSecondAfter = ConstantData.GameStates.Go;
                stateFirstAfter = ConstantData.GameStates.Wait;
            }
        }

        private static TcpClient AcceptClient(TcpListener server, string teame)
        {
            TcpClient player = server.AcceptTcpClient();
            Logger.Log($"player {teame} connected from { player.Client.RemoteEndPoint}");
            ConnectionTools.SendResponce(player, teame);

            return player;
        }

        private static void ClearField(CellValue[,] field)
        {
            for (int i = 0; i < RowCount; i++)
                for (int j = 0; j < ColumnCount; j++)
                    field[i, j] = CellValue.Empty;
        }

        private static CellValue[,] RandomField(CellValue[,] field, Random random, int shipCount = 5)
        {
            //Console.OutputEncoding = Encoding.GetEncoding(866);

            for (int i = 0; i < shipCount; i++)
            {
                int iParameter = random.Next(0, RowCount - 1);
                int jParameter = random.Next(0, ColumnCount - 1);

                if (field[iParameter, jParameter] != CellValue.Ship)
                {
                    field[iParameter, jParameter] = CellValue.Ship;
                }
                else if (field[iParameter, jParameter] == CellValue.Ship)
                {
                    i--;
                }
            }

            return field;
        }
    }


    public enum CellValue
    {
        Ship = 'X',
        Empty = '.',
        First = '1',
        FirstHit = '⓵',
        Second = '2',
        SecondHit = '⓶'
    }

    public enum GameStatus
    {
        Play = -1,
        Draw = 0,
        WinFirst = 1,
        WinSecond = 2
    }
}
