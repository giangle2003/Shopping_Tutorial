namespace Shopping_Tutorial.Models
{
    public class Paginate
    {
        public int TotalItems { get; private set; } //tổng số sản phẩm
        public int PageSize { get; private set; } // tổng số sản phẩm trên 1 trang
        public int CurrentPage { get; private set; } // trang hiện tại
        public int TotalPages { get; private set; } // tổng số trang
        public int StartPage { get; private set; }  // trang bắt đầu
        public int EndPage { get; private set; }    // trang kết thúc

        public Paginate()
        {

        }

        public Paginate(int totalItems, int page, int pageSize = 10)
        {
            //làm tròn tổng items/10 items trên 1 trang; VD 16 items/10 = 1.6 => 2 trang
            int totalPages = (int)Math.Ceiling((decimal)totalItems / (decimal)pageSize);

            //page hiên tại  = 1
            int currentPage = page;

            //trang bắt đầu trừ 5 button
            int startPage = currentPage - 5;
            //trang kết thúc cộng 4 button
            int endPage = currentPage + 4;

            if (startPage <= 0)
            {
                //nếu trang bắt đầu <= 0 thì trang bắt đầu = 1
                endPage -= (startPage - 1);
                startPage = 1;
            }
            if (endPage > totalPages)
            {
                //nếu trang kết thúc > tổng số trang thì trang kết thúc = tổng số trang
                endPage = totalPages;
                if (endPage > 10)
                {
                    //nếu tổng số trang > 10 thì trang bắt đầu = tổng số trang - 9
                    startPage = endPage - 9;
                }
            }

            TotalItems = totalItems;
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalPages = totalPages;
            StartPage = startPage;
            EndPage = endPage;
        }
    }
}
