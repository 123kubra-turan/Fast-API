using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace Stok_Kontrol_V2
{
    public partial class Form1 : Form
    {
        private MySqlConnection mySqlConnection;

        public Form1()
        {
            InitializeComponent();  // Bu satır InitializeComponent metodunu çağırır.

            string mySqlCon = "server=127.0.0.1;user=root;database=stokkontroldb;password=";
            mySqlConnection = new MySqlConnection(mySqlCon);

            try
            {
                mySqlConnection.Open();
                MessageBox.Show("Bağlantı başarılı");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                mySqlConnection.Close();
            }

            VeriYukle(); // DataGridView'i güncelle
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Form yükleme olayı burada işlenebilir
        }

        // Veritabanından verileri yüklemek için metod
        private void VeriYukle()
        {
            try
            {
                // mySqlConnection.Open(); // Bu satırı kaldırın
                
                string query = "SELECT * FROM urunler";
                MySqlDataAdapter adapter = new MySqlDataAdapter(query, mySqlConnection);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                dataGridView1.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veri yüklenirken hata oluştu: " + ex.Message);
            }
            //finally
            //{
            // mySqlConnection.Close(); // Bu satırı kaldırın
            //}
        }

        private void ekle_Click_1(object sender, EventArgs e)
        {
            string query = "INSERT INTO urunler (UrunAdi, Kategori, Miktar, Fiyat, KarOrani) VALUES (@UrunAdi, @Kategori, @Miktar, @Fiyat, @KarOrani)";

            using (MySqlCommand cmd = new MySqlCommand(query, mySqlConnection))
            {
                cmd.Parameters.AddWithValue("@UrunAdi", textBox1.Text);
                cmd.Parameters.AddWithValue("@Kategori", textBox2.Text);
                cmd.Parameters.AddWithValue("@Miktar", Convert.ToInt32(textBox3.Text));
                cmd.Parameters.AddWithValue("@Fiyat", Convert.ToDecimal(textBox4.Text));
                cmd.Parameters.AddWithValue("@KarOrani", Convert.ToDecimal(textBox5.Text));

                try
                {
                    mySqlConnection.Open();
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Ürün başarıyla eklendi.");

                    // DataGridView'i güncelle
                    mySqlConnection.Close(); // Bağlantıyı kapatın
                    VeriYukle(); // Ardından VeriYukle metodunu çağırın
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hata: " + ex.Message);
                }
                finally
                {
                    mySqlConnection.Close();
                }
            }
        }

        private void sil_Click_1(object sender, EventArgs e)
        {
            // DataGridView'den seçili satırın ID'sini al
            int id = Convert.ToInt32(dataGridView1.CurrentRow.Cells["UrunId"].Value);

            string query = "DELETE FROM urunler WHERE Urunid=@UrunId";

            using (MySqlCommand cmd = new MySqlCommand(query, mySqlConnection))
            {
                cmd.Parameters.AddWithValue("@UrunId", id);

                try
                {
                    mySqlConnection.Open();
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Ürün başarıyla silindi.");

                    // DataGridView'i güncelle
                    
                    mySqlConnection.Close(); // Bağlantıyı kapatın
                    VeriYukle(); // Ardından VeriYukle metodunu çağırın
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hata: " + ex.Message);
                }
                finally
                {
                    mySqlConnection.Close();
                }
            }
        }

        private void duzenle_Click_1(object sender, EventArgs e)
        {
            // DataGridView'den seçili satırın ID'sini al
            int id = Convert.ToInt32(dataGridView1.CurrentRow.Cells["UrunId"].Value);

            string query = "UPDATE urunler SET UrunAdi=@UrunAdi, Kategori=@Kategori, Miktar=@Miktar, Fiyat=@Fiyat, KarOrani=@KarOrani WHERE Urunid=@UrunId";

            using (MySqlCommand cmd = new MySqlCommand(query, mySqlConnection))
            {
                cmd.Parameters.AddWithValue("@UrunId", id);
                cmd.Parameters.AddWithValue("@UrunAdi", textBox1.Text);
                cmd.Parameters.AddWithValue("@Kategori", textBox2.Text);
                cmd.Parameters.AddWithValue("@Miktar", Convert.ToInt32(textBox3.Text));
                cmd.Parameters.AddWithValue("@Fiyat", Convert.ToDecimal(textBox4.Text));
                cmd.Parameters.AddWithValue("@KarOrani", Convert.ToDecimal(textBox5.Text));

                try
                {
                    mySqlConnection.Open();
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Ürün başarıyla güncellendi.");

                    // DataGridView'i güncelle
                    mySqlConnection.Close(); // Bağlantıyı kapatın
                    VeriYukle(); // Ardından VeriYukle metodunu çağırın
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hata: " + ex.Message);
                }
                finally
                {
                    mySqlConnection.Close();
                }
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Seçili satır indeksini alın
            int rowIndex = e.RowIndex;

            // Satır indeksi geçerli bir satır olup olmadığını kontrol edin
            if (rowIndex >= 0)
            {
                // Seçili satırdaki hücre değerlerini alın
                DataGridViewRow selectedRow = dataGridView1.Rows[rowIndex];

                // Her bir TextBox'a uygun hücre değerini atayın
                textBox1.Text = selectedRow.Cells[1].Value?.ToString();
                textBox2.Text = selectedRow.Cells[2].Value?.ToString();
                textBox3.Text = selectedRow.Cells[3].Value?.ToString();
                textBox4.Text = selectedRow.Cells[4].Value?.ToString();
                textBox5.Text = selectedRow.Cells[5].Value?.ToString();

                // Ekstra sütunlar varsa, burada ek TextBox'lar için devam edebilirsiniz
            }
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            string searchText = textBox6.Text;

            // Veritabanında arama yap ve sonuçları DataGridView'e doldur
            SearchDatabase(searchText);
        }

        private void SearchDatabase(string searchText)
        {
            try
            {
                mySqlConnection.Open();
                string query = @"SELECT * FROM urunler
                                 WHERE UrunAdi LIKE @SearchText
                                 OR Kategori LIKE @SearchText
                                 OR KarOrani LIKE @SearchText";

                using (MySqlCommand command = new MySqlCommand(query, mySqlConnection))
                {
                    // Arama metni için parametre ekleyin
                    command.Parameters.AddWithValue("@SearchText", "%" + searchText + "%");

                    MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    // DataGridView'i doldur
                    dataGridView1.DataSource = dataTable;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Arama yapılırken hata oluştu: " + ex.Message);
            }
            finally
            {
                mySqlConnection.Close();
            }
        }
    }
}
