I/ Tài liệu tham khảo
[1] Holt, Charles C. (1957). "Forecasting Trends and Seasonal by Exponentially Weighted Averages". Office of Naval Research Memorandum. 52. reprinted in Holt, Charles C. (January–March 2004). "Forecasting Trends and Seasonal by Exponentially Weighted Averages". International Journal of Forecasting. 20 (1): 5–10. doi:10.1016/j.ijforecast.2003.09.015.
[2] http://people.duke.edu/~rnau/Notes_on_forecasting_with_moving_averages--Robert_Nau.pdf

II/
	
Phương pháp thống kê dự báo dùng để ước lượng dữ liệu trong tương lai dựa vào dữ liệu thu thập được ở hiện tại và quá khứ,
 khi không có các quy luật tự nhiên của quá trình phát sinh ra dữ liệu. (theo Robert Nau)

Bài toán ở đây tìm cách dự báo dữ liệu doanh thu bán hàng của tuần kế tiếp dựa trên doanh thu của các tuần trước đó.
 Dữ liệu của bài toán thuộc lại chuỗi thời gian, có đặc điểm là ít điểm quan sát (ngắn), khoảng 3 đến 20 điểm,
 hơn nữa lại không phù hợp khi giả định chuỗi có tính ổn định
 (trung bình và phương sai của doanh số là hằng số từ tuần này qua tuần khác).
 
Vì vậy, mô hình đơn giản Random Walk (như gợi ý trong [2], trang 2) và Linear Exponential Smoothing
 (còn gọi là Exponential Weighted Moving Average, tương tự như trong [1]) được áp dụng.

Mô hình LES có thể mô tả xu hướng (trend), và cập nhật qua từng bước nên có thể áp dụng khi có dữ liệu mới.

III/ Thực nghiệm
Thực nghiệm trên 3 chuỗi ở Sheet 2, dòng 99, 100, 101. Cài phần mềm R ở r-project.org, sau đó cài RStudio ở https://www.rstudio.com/products/rstudio/download3/
Dùng RStudio mở file commodity.Rproj.

Rplot.png biểu diễn 3 chuỗi theo thứ tự từ trên xuống.

1/Random Walk
2/Linear exponential smoothing

Trên chuỗi x101: RMSE = 71071.13, MAE = 64503.23, MAPE = 0.1787522