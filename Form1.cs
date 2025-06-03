using System;
using System.Configuration;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace Stok_Kontrol_V2
{
    public partial class Form1 : Form
    {
        // Bağlantı stringini config'den okuyoruz.
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["StokDb"].ConnectionString;

        public Form1()
        {
            InitializeComponent();
            // Form yüklenirken verileri asenkron yükle
            this.Load += async (s, e) => await VeriYukleAsync();
        }

        // Veritabanından ürünleri asenkron yükler
        private async Task VeriYukleAsync()
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = "SELECT * FROM urunler";
                    using (var adapter = new MySqlDataAdapter(query, connection))
                    {
                        var dt = new DataTable();
                        adapter.Fill(dt);
                        dataGridView1.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veri yüklenirken hata oluştu: " + ex.Message);
            }
        }

        // Ürün ekle butonu
        private async void btnEkle_Click(object sender, EventArgs e)
        {
            if (!GirdiKontrol())
                return;

            var query = @"INSERT INTO urunler (UrunAdi, Kategori, Miktar, Fiyat, KarOrani)
                          VALUES (@UrunAdi, @Kategori, @Miktar, @Fiyat, @KarOrani)";
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UrunAdi", textBox1.Text.Trim());
                    command.Parameters.AddWithValue("@Kategori", textBox2.Text.Trim());
                    command.Parameters.AddWithValue("@Miktar", int.Parse(textBox3.Text));
                    command.Parameters.AddWithValue("@Fiyat", decimal.Parse(textBox4.Text));
                    command.Parameters.AddWithValue("@KarOrani", decimal.Parse(textBox5.Text));
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
                MessageBox.Show("Ürün başarıyla eklendi.");
                await VeriYukleAsync();
                Temizle();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }

        // Ürün sil (seçili satırdan)
        private async void btnSil_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Lütfen silinecek ürünü seçin.");
                return;
            }

            int id = Convert.ToInt32(dataGridView1.CurrentRow.Cells["UrunId"].Value);

            var query = "DELETE FROM urunler WHERE UrunId = @UrunId";
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UrunId", id);
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
                MessageBox.Show("Ürün başarıyla silindi.");
                await VeriYukleAsync();
                Temizle();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }

        // Ürün düzenle
        private async void btnDuzenle_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Lütfen düzenlenecek ürünü seçin.");
                return;
            }
            if (!GirdiKontrol())
                return;

            int id = Convert.ToInt32(dataGridView1.CurrentRow.Cells["UrunId"].Value);

            var query = @"UPDATE urunler SET UrunAdi=@UrunAdi, Kategori=@Kategori, Miktar=@Miktar, Fiyat=@Fiyat, KarOrani=@KarOrani
                          WHERE UrunId=@UrunId";
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UrunAdi", textBox1.Text.Trim());
                    command.Parameters.AddWithValue("@Kategori", textBox2.Text.Trim());
                    command.Parameters.AddWithValue("@Miktar", int.Parse(textBox3.Text));
                    command.Parameters.AddWithValue("@Fiyat", decimal.Parse(textBox4.Text));
                    command.Parameters.AddWithValue("@KarOrani", decimal.Parse(textBox5.Text));
                    command.Parameters.AddWithValue("@UrunId", id);
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
                MessageBox.Show("Ürün başarıyla güncellendi.");
                await VeriYukleAsync();
                Temizle();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }

        // Satır tıklanınca textbox'lara değerleri aktar
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dataGridView1.Rows[e.RowIndex];
            textBox1.Text = row.Cells["UrunAdi"].Value?.ToString() ?? "";
            textBox2.Text = row.Cells["Kategori"].Value?.ToString() ?? "";
            textBox3.Text = row.Cells["Miktar"].Value?.ToString() ?? "";
            textBox4.Text = row.Cells["Fiyat"].Value?.ToString() ?? "";
            textBox5.Text = row.Cells["KarOrani"].Value?.ToString() ?? "";
        }

        // Arama kutusu değişince arama yap
        private async void textBox6_TextChanged(object sender, EventArgs e)
        {
            await SearchDatabaseAsync(textBox6.Text.Trim());
        }

        // Veritabanında arama (asenkron)
        private async Task SearchDatabaseAsync(string searchText)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    var query = @"SELECT * FROM urunler
                                  WHERE UrunAdi LIKE @SearchText
                                     OR Kategori LIKE @SearchText
                                     OR KarOrani LIKE @SearchText";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SearchText", "%" + searchText + "%");
                        using (var adapter = new MySqlDataAdapter(command))
                        {
                            var dt = new DataTable();
                            adapter.Fill(dt);
                            dataGridView1.DataSource = dt;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Arama yapılırken hata oluştu: " + ex.Message);
            }
        }

        // Kullanıcı girişlerini kontrol eden yardımcı fonksiyon
        private bool GirdiKontrol()
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) ||
                string.IsNullOrWhiteSpace(textBox2.Text) ||
                !int.TryParse(textBox3.Text, out _) ||
                !decimal.TryParse(textBox4.Text, out _) ||
                !decimal.TryParse(textBox5.Text, out _))
            {
                MessageBox.Show("Lütfen tüm alanları doğru doldurun!");
                return false;
            }
            return true;
        }

        // Formdaki textbox'ları temizler
        private void Temizle()
        {
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            textBox4.Clear();
            textBox5.Clear();
        }
    }
}
