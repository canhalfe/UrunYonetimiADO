using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UrunYonetimiADO.Models;

namespace UrunYonetimiADO
{
    public partial class Form1 : Form
    {
        string conStr = @"server=(localdb)\MSSQLLocalDB; Database=UrunYonetimiAdoDB; Trusted_Connection=True;";
        BindingList<Urun> urunler = new BindingList<Urun>();
        public Form1()
        {
            VeriTabaniYoksaOlustur();
            InitializeComponent();
            dgvUrunler.AutoGenerateColumns = false;
            dgvUrunler.DataSource = urunler;
            UrunleriListele();
        }

        private void UrunleriListele()
        {
            using (var con = BaglantiOlustur())
            {
                var cmd = new SqlCommand("Select Id,UrunAd,BirimFiyat from Urunler", con);
                var dr = cmd.ExecuteReader();

                urunler.Clear();
                while (dr.Read())
                {
                    urunler.Add(new Urun
                    {
                        Id = (int)dr["Id"],
                        UrunAd = (string)dr["UrunAd"],
                        BirimFiyat = (decimal)dr["BirimFiyat"]
                    });
                }
            }
        }

        private SqlConnection BaglantiOlustur()
        {
            var con = new SqlConnection(conStr);
            con.Open();
            return con;
        }

        private void VeriTabaniYoksaOlustur()
        {
            //connection açıldıktan sonra ne zaman işlem yapılırsa işlem sonrası kapatsın istiyorsan 
            //using içinde;

            using (var con = new SqlConnection(@"server=(localdb)\MSSQLLocalDB; Database=master; Trusted_Connection=True;"))
            {
                con.Open();  //connection'ı aç
                SqlCommand cmd = new SqlCommand(@"
                If (DB_ID('UrunYonetimiAdoDB') is null) 
                Create Database UrunYonetimiAdoDb;", con); //cmd'yi aç
                cmd.ExecuteNonQuery();   //cmd'yi çalıştır.
            }
            using (var con = BaglantiOlustur())
            {
                SqlCommand cmd = new SqlCommand(@"
                If OBJECT_ID(N'dbo.Urunler', N'U') is null
                Create Table Urunler
                (
	                Id int primary key identity,
	                UrunAd Nvarchar(100) not null,
	                BirimFiyat Decimal(18,2) not null
                );", con);
                cmd.ExecuteNonQuery();
            }
        }

        private void btnEkle_Click(object sender, EventArgs e)
        {
            string urunAd = txtUrunAd.Text.Trim();
            decimal birimFiyat = nudBirimFiyat.Value;

            if (urunAd == "")
            {
                MessageBox.Show("Urun adı girmelisiniz");
                return;
            }

            using (var con = BaglantiOlustur())
            {
                var cmd = new SqlCommand("Insert into Urunler(UrunAd,BirimFiyat) Values (@p1,@p2);", con);
                cmd.Parameters.AddWithValue("@p1", urunAd);
                cmd.Parameters.AddWithValue("@p2", birimFiyat);
                cmd.ExecuteNonQuery();
            }
            UrunleriListele();
            txtUrunAd.Clear();
        }

        private void btnDuzenle_Click(object sender, EventArgs e)
        {
            if (dgvUrunler.SelectedRows.Count > 0)
            {
                Urun seciliUrun = (Urun)dgvUrunler.SelectedRows[0].DataBoundItem;
                DuzenleForm frmDuzenle = new DuzenleForm(seciliUrun);  //düzenleform'a seçili ürünü gönderiyoruz.
                frmDuzenle.UrunDuzenlendi += FrmDuzenle_UrunDuzenlendi;
                
                frmDuzenle.Show(this);
            }
        }

        private void FrmDuzenle_UrunDuzenlendi(object sender, UrunDuzenlendiEventArgs e)
        {
            using (var con = BaglantiOlustur())
            {
                var cmd = new SqlCommand("Update Urunler Set UrunAd = @p1, BirimFiyat = @p2 where Id=@p0", con);
                cmd.Parameters.AddWithValue("@p0", e.YeniUrun.Id);
                cmd.Parameters.AddWithValue("@p1", e.YeniUrun.UrunAd);
                cmd.Parameters.AddWithValue("@p2", e.YeniUrun.BirimFiyat);
                cmd.ExecuteNonQuery();
            }
            UrunleriListele();

            //En son düzenlenen ürünü seç
            dgvUrunler.ClearSelection();
            foreach (DataGridViewRow row in dgvUrunler.Rows)
            {
                Urun siradaki = (Urun)row.DataBoundItem;

                if (siradaki.Id == e.YeniUrun.Id)
                {
                    row.Selected = true;
                    break;
                }
            }
        }

        private void btnSil_Click(object sender, EventArgs e)
        {
            if (dgvUrunler.SelectedRows.Count > 0)
            {
                int seciliIndeks = dgvUrunler.SelectedRows[0].Index;
                Urun seciliUrun = (Urun)dgvUrunler.SelectedRows[0].DataBoundItem;
                using (var con = BaglantiOlustur())
                {
                    var cmd = new SqlCommand("Delete From Urunler Where Id = @p1;", con);
                    cmd.Parameters.AddWithValue("@p1", seciliUrun.Id);
                    cmd.ExecuteNonQuery();
                }
                UrunleriListele();

                //bir satır silince,silinmiş satırla aynı indeksteki satır seçili gelsin;
                if (dgvUrunler.Rows.Count > 0)
                {
                    dgvUrunler.ClearSelection();

                    int secilecekIndeks = seciliIndeks >= dgvUrunler.Rows.Count ?
                        dgvUrunler.Rows.Count - 1
                        : seciliIndeks;

                    dgvUrunler.Rows[secilecekIndeks].Selected = true;
                }

            }
        }
    }
}
