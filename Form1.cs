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
        // 1. Bağlantı cümlesi config'den okunuyor
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["StokDb"].ConnectionString;

        public Form1()
        {
            InitializeComponent();
            // Form açılırken ürünleri yükle
            this.Load += async (s, e) => await VeriYukleAsync();
        }

        // 2. Ürünleri yükler (listeleme)
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

        // 3. Ürün ekler
        private async void btnEkle_Click(object sender, EventArgs e)
        {
            if (!GirdiKontrol()) return;

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

        // 4. Ürün siler
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

        // 5. Ürün günceller
        private async void btnDuzenle_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Lütfen düzenlenecek ürünü seçin.");
                return;
            }
            if (!GirdiKontrol()) return;

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

        // 6. Satır tıklayınca textbox'lara değer aktarır
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

        // 7. Arama kutusu değişince arama yapar
        private async void textBox6_TextChanged(object sender, EventArgs e)
        {
            await SearchDatabaseAsync(textBox6.Text.Trim());
        }

        // 8. Arama fonksiyonu
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

        // 9. Girdi kontrolü (hatalı veri önleme)
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

        // 10. Form temizleme fonksiyonu
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
