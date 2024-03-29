﻿using banks.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace banks
{
   
    public interface IClient
    {
        List<Client> Clients { get; }
        void AddClient(string name, DateTime date, string email, string phone, string address);
        void EditClient(string name, DateTime date, string email, string phone, string address, int id);
    }
    public interface IAccount
    {
        List<Account> Accounts { get; }
        List<Account> ClAcc(int id);
        void AddAccount(int clientId, decimal balance);
        void ChangeAccountStatus(Account acc);
        void Money(int id, decimal amount);
    }
    public interface ILoan
    {
        List<Loan> Loans { get; }
        List<Loan> DateLoan(DateTime start, DateTime end, string c);
        List<Loan> AccLoan(int id);
        void AddLoan(int id, decimal am, DateTime end, decimal per);
        void StatusLoan();
        bool CloseLoan(Loan l);

    }
    public interface IDeposit
    {
        List<Deposit> Deposits { get; }
        List<Deposit> DateDep(DateTime start, DateTime end, string c);
        List<Deposit> AccDep(int id);
        void AddDeposit(int id, decimal am, DateTime end, decimal per);
        void StatusDeposit();
        void CloseDeposit(Deposit d);
    }
    public interface ITransaction
    {
        List<Transaction> Transactions { get; }
        List<Transaction> DateTran(DateTime start, DateTime end);
        List<Transaction> AccTran(int id);
        void AddTran(int id, int to, decimal amount);

    }
    public interface IGetData
    {
        static string Path;
        void LoadData();
        void SaveData();
    }
    public class BankData: IAccount, IClient, ILoan, IDeposit, ITransaction
    {
        
        public List<Client> Clients { get; set; }
        public List<Account> Accounts { get; set; }
        public List<Transaction> Transactions { get; set; }
        public List<Loan> Loans { get; set; }
        public List<Deposit> Deposits { get; set; }
        public virtual void SaveData()
        {
        }
        public void AddClient(string name, DateTime date, string email, string phone, string address)
        { 
            int w = 1;
            if (Clients.Count != 0)
                w += Clients.Max(u => u.Id);
            var cl = new Client
            {
                Name = name,
                BirthDate = date,
                Phone = phone,
                Address = address,
                Email = email,
                Id = w
            };
            Clients.Add(cl);
            SaveData();
        }
        public void EditClient(string name, DateTime date, string email, string phone, string address, int id)
        {
            var cl = new Client
            {
                Name = name,
                BirthDate = date,
                Phone = phone,
                Address = address,
                Email = email,
                Id = id
            };
            Clients.RemoveAll(u => u.Id == id);
            Clients.Add(cl);
            Clients = Clients.OrderBy(u => u.Id).ToList();
            SaveData();
        }
        public void AddLoan(int id, decimal am, DateTime end, decimal percent)
        { 
            int w = 1;
            if (Loans.Count != 0)
                w += Loans.Max(u => u.LoanId);
            var lo = new Loan
            {
                LoanId = w,
                Amount = am,
                StartDate = DateTime.Now.Date,
                EndDate = end,
                AccId = id,
                Status = "Active",
                Percent = percent
            };
            Money(id, am);
            Loans.Add(lo);
            SaveData();
        }
        public void AddDeposit(int id, decimal amount, DateTime endDate, decimal percent)
        {
            int w = 1;
            if (Deposits.Count != 0)
                w += Deposits.Max(u => u.DepId);
            var dep = new Deposit
            { 
                AccId = id,
                Amount = amount,
                StartDate = DateTime.Now.Date,
                EndDate = endDate,
                Percent = percent,
                Status = "Active",
                DepId = w
            };
            Money(id, -amount);
            Deposits.Add(dep);
            SaveData();
        }
        public void AddAccount(int clientId, decimal balance)
        {
            int w = 1;
            if (Accounts.Count != 0)
                w += Accounts.Max(u => u.AccId);
            var ac = new Account
            {
                ClientId = clientId,
                AccId = w,
                Balance = balance,
                Status = true
            };
            Accounts.Add(ac);
            SaveData();
        }
        public void AddTran(int id, int to, decimal amount)
        {
            int w = 1;
            if (Transactions.Count != 0)
                w += Transactions.Max(u => u.TranId);
            var trab = new Transaction
            {
                From = id,
                To = to,
                Amount = amount,
                Date = DateTime.Now.Date,
                TranId = w
            };
            Money(id, -amount);
            Money(to, amount);
            Transactions.Add(trab);
            SaveData();
        }
        public void ChangeAccountStatus(Account acc)
        {
            if (acc.Status == false)
                acc.Status = true;
            else
                acc.Status = false;
            Accounts.RemoveAll(u => u.AccId == acc.AccId);
            Accounts.Add(acc);
            SaveData();
        }
        public List<Loan> DateLoan(DateTime start, DateTime end, string c)
        {
            if (c == "All")
                return Loans.FindAll(u => u.StartDate >= start && u.EndDate <= end);
            return Loans.FindAll(u => u.StartDate >= start && u.EndDate <= end && u.Status == c);
        }
        public List<Deposit> DateDep(DateTime start, DateTime end, string c)
        {
            if (c == "All")
                return Deposits.FindAll(u => u.StartDate >= start && u.EndDate <= end);
            return Deposits.FindAll(u => u.StartDate >= start && u.EndDate <= end && u.Status == c);
        }
        public List<Transaction> DateTran(DateTime start, DateTime end)
        {
            return Transactions.FindAll(u => u.Date >= start && u.Date <= end);
        }
        public List<Loan> AccLoan(int id)
        {
            return Loans.FindAll(u => u.AccId == id);
        }
        public List<Deposit> AccDep(int id)
        {
            return Deposits.FindAll(u => u.AccId == id);
        }
        public List<Transaction> AccTran(int id)
        {
            return Transactions.FindAll(u => u.From == id || u.To == id);
        }
        public List<Account> ClAcc(int id)
        {
            return Accounts.FindAll(u => u.ClientId == id);
        }
        public void StatusDeposit()
        {
            var depo = Deposits.FindAll(u => u.EndDate < DateTime.Now && u.Status == "Active");
            foreach (Deposit el in depo)
            {
                Loans.RemoveAll(u => u.LoanId == el.DepId);
                el.Status = "Closed";
                Deposits.Add(el);
                decimal sum = el.Amount * (1 + (el.Percent / 100));
                Money(el.AccId, sum);
            }
            SaveData();
        }
        public void StatusLoan()
        {
            var los = Loans.FindAll(u => u.EndDate < DateTime.Now && u.Status == "Active");
            foreach (Loan el in los)
            {
                Loans.RemoveAll(u => u.LoanId == el.LoanId);
                el.Status = "Expired";
                Loans.Add(el);
            }
            SaveData();
        }
        public void CloseDeposit(Deposit d)
        {
            Deposit de = Deposits.First(u => u.DepId == d.DepId);
            de.Status = "Closed";
            Deposits.RemoveAll(u => u.DepId == d.DepId);
            Deposits.Add(de);
            int month = MonthDifference(de.StartDate);
            decimal depAmount = Math.Round(de.Amount * (decimal)Math.Pow((double)de.Percent / 100 + 1, month - 1), 2);
            Money(d.AccId, depAmount);
            SaveData();
        }
        private int MonthDifference(DateTime dat)
        {
            int month = (DateTime.Now.Year - dat.Year) * 12 + DateTime.Now.Month - dat.Month;
            if (DateTime.Now.Day >= dat.Day)
                month += 1;
            return month;
        }
        public bool CloseLoan(Loan l)
        {
            Loan lo = Loans.First(u => u.LoanId == l.LoanId);
            Account acc = Accounts.First(u => u.AccId == l.AccId);
            int month = MonthDifference(lo.StartDate);
            var loanAmount = Math.Round(lo.Amount * (decimal)Math.Pow((double)lo.Percent / 100 + 1, month), 2);
            if (acc.Balance < loanAmount)
                return false;
            else
            {
                lo.Status = "Repaid";
                Loans.RemoveAll(u => u.LoanId == l.LoanId);
                Loans.Add(lo);
                Money(l.AccId, -loanAmount);
                SaveData();
                return true;
            }    
        }
        public void Money(int id, decimal amount)
        {
            Account acc = Accounts.First(u => u.AccId == id);
            Accounts.RemoveAll(u => u.AccId == id);
            acc.Balance = acc.Balance + amount;
            Accounts.Add(acc);
            SaveData();
        }
    }
    public class BankDataJson: BankData, IGetData
    {
        
        static string Path = "../../../../banks/Data/BankData.json";
        
        public BankDataJson()
        {
            LoadData();
        }
        public void Serialize<T>(T data)
        {
            using (var sw = new StreamWriter(Path))
            {
                using (var jsonWriter = new JsonTextWriter(sw))
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(jsonWriter, data);
                }
            }
        }
        public T Deserialize<T>()
        {
            using (var sr = new StreamReader(Path))
            {
                using (var jsonReader = new JsonTextReader(sr))
                {
                    var serializer = new JsonSerializer();
                    return serializer.Deserialize<T>(jsonReader);
                }
            }
        }
        public void LoadData()
        {
            var data = Deserialize<BankData>();
            Clients = data.Clients ?? new List<Client>();
            Deposits = data.Deposits ?? new List<Deposit>();
            Transactions = data.Transactions ?? new List<Transaction>();
            Accounts = data.Accounts ?? new List<Account>();
            Loans = data.Loans ?? new List<Loan>();

        }
        public override void SaveData()
        {
            var data = new BankData
            {
                Clients = Clients,
                Deposits = Deposits,
                Accounts = Accounts,
                Loans = Loans,
                Transactions = Transactions
            };
            Serialize(data);
        }
    }
}
