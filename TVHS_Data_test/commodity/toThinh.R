par(mfcol = c(3,1))
x99 <- c(323529.4118, 320833.3333, 346638.6555, 422899.1597, 434453.7815,443697.479,360504.2017, 355882.3529, 231092.437, 291176.4706, 291176.4706, 499159.6639)
x <- x99
plot(x, main = "x99")
lines(x)

x100 <- c(442156.8627, 485294.1176, 438151.2605, 543529.4118, 513025.2101, 557394.958, 549075.6303, 565714.2857, 632268.9076, 575073.5294, 516521.7391, 489176.4706)
x <- x100
plot(x, main = "x100")
lines(x)

x101 <- c(317581.6993, 354117.6471, 439971.9888, 373781.5126, 401036.4146, 473934.4262, 396557.377, 495130.719, 423252.5952, 313431.3725, 280336.1345, 305254.902)
x <- x101
plot(x, main = "x101")
lines(x)

#linear exponential smoothing
x <- x99
m <- HoltWinters(x, gamma = F)
plot(m)

x <- x100
m <- HoltWinters(x, gamma = F)
plot(m)

#calculate RMSE, MAE, MAPE
x <- x101
m <- HoltWinters(x, gamma = F)
plot(m)
pred <- as.vector(m$fitted[,1])
x <- x[3:length(x)]
error <- pred - x
RMSE <- sqrt(sum(error*error)/length(error))
RMSE
MAE <- sum(abs(error)) / length(error)
MAE
MAPE <- sum(abs(error)/x)/length(error)
MAPE
