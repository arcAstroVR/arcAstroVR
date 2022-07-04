using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class aAV_GIS : MonoBehaviour
{
	//計算式参照：国土地理院「測量計算サイト」https://vldb.gsi.go.jp/sokuchi/surveycalc/main.html
	//C#プログラム参照：https://www.kobiwa.jp/2017/03/22/post-286/

	const double daa = 6378137; //長半径
	const double dF = 298.257222101d; //逆扁平率
	const double dM0 = 0.9996; //縮尺係数(UTMの場合→0.9996、平面直角座標系(世界測地系)の場合→0.9999)
	private double[][] jp19list = new double[20][];
	
	// Start is called before the first frame update
	void Start()
	{
		//平面直角座標19系の原点定義
		jp19list[1] = new double[] {129.5d,33d};
		jp19list[2] = new double[] {131d,33d};
		jp19list[3] = new double[] {132.16666666666669d,36d};
		jp19list[4] = new double[] {133.5d,33d};
		jp19list[5] = new double[] {134.33333333333334d,36d};
		jp19list[6] = new double[] {136d,36d};
		jp19list[7] = new double[] {137.16666666666669d,36d};
		jp19list[8] = new double[] {138.5d,36d};
		jp19list[9] = new double[] {139.83333333333334d,36d};
		jp19list[10] = new double[] {140.83333333333334d,40d};
		jp19list[11] = new double[] {140.25d,44d};
		jp19list[12] = new double[] {142.25d,44d};
		jp19list[13] = new double[] {144.25d,44d};
		jp19list[14] = new double[] {142d,26d};
		jp19list[15] = new double[] {127.5d,26d};
		jp19list[16] = new double[] {124d,26d};
		jp19list[17] = new double[] {131d,26d};
		jp19list[18] = new double[] {136d,20d};
		jp19list[19] = new double[] {154d,26d};

/*
		//演算確認テスト（吉野ヶ里遺跡）
		double center_lon=130.386306d;
		double center_lat=33.326917d;
		Debug.Log("原点座標（経度緯度）"+center_lon+", "+center_lat);
		double lon=130.386619d;
		double lat=33.328904d;
		Debug.Log("入力座標（経度緯度）"+lon+", "+lat);
		double [] EN=LonLat2EN(lon, lat, center_lon, center_lat, dM0);
		Debug.Log("平面直角座標（汎用）EN="+EN[0]+", "+EN[1]);
		double [] LL=EN2LonLat(EN[0], EN[1], center_lon, center_lat, dM0);
		Debug.Log("逆演算（汎用）lonlat="+LL[0]+", "+LL[1]);
		int zone = 2;
		double [] XY=LonLat2JP(lon, lat, zone);
		Debug.Log("平面直角座標19系（"+zone+"系）XY="+XY[1]+", "+XY[0]);
		LL=JP2LonLat(XY[0], XY[1], zone);
		Debug.Log("逆演算（19系）lonlat="+LL[0]+", "+LL[1]);
		zone = 52;
		XY=LonLat2UTM(lon, lat, zone);
		Debug.Log("平面直角座標UTM（ゾーン"+zone+"）XY="+XY[0]+", "+XY[1]);
		LL=UTM2LonLat(XY[0], XY[1], zone);
		Debug.Log("逆演算（UTM）lonlat="+LL[0]+", "+LL[1]);
*/
	}

	// 緯度経度→平面直角座標（汎用）
	// double [E座標(m), N座標(m), 真北偏差(°)] = LonLat2EN(経度(°), 緯度(°), 基準点経度(°), 基準点緯度(°), 縮尺係数)
	public double [] LonLat2EN(double Lon, double Lat, double Lon0, double Lat0, double dM0)
	{
		double dn = 1d / (2 * dF - 1);
		Lon = Deg2Rad(Lon); Lat = Deg2Rad(Lat); Lon0 = Deg2Rad(Lon0); Lat0 = Deg2Rad(Lat0);

		double dt = Math.Sinh(atanh(Math.Sin(Lat)) - (2 * Math.Sqrt(dn)) / (1 + dn) * atanh(2 * Math.Sqrt(dn) / (1 + dn) * Math.Sin(Lat)));
		double dtb = Math.Sqrt(1 + Math.Pow(dt, 2));
		double dLmc = Math.Cos(Lon - Lon0);
		double dLms = Math.Sin(Lon - Lon0);
		double dXi = Math.Atan(dt / dLmc);
		double dEt = atanh(dLms / dtb);

		//α1→0～α5→4
		double[] dal = new double[6];
		dal[0] = 0;
		dal[1] = 1d / 2d * dn - 2d / 3d * Math.Pow(dn, 2) + 5d / 16d * Math.Pow(dn, 3) + 41d / 180d * Math.Pow(dn, 4) - 127d / 288d * Math.Pow(dn, 5);
		dal[2] = 13d / 48d * Math.Pow(dn, 2) - 3d / 5d * Math.Pow(dn, 3) + 557d / 1440d * Math.Pow(dn, 4) + 281d / 630d * Math.Pow(dn, 5);
		dal[3] = 61d / 240d * Math.Pow(dn, 3) - 103d / 140d * Math.Pow(dn, 4) + 15061d / 26880d * Math.Pow(dn, 5);
		dal[4] = 49561d / 161280d * Math.Pow(dn, 4) - 179d / 168d * Math.Pow(dn, 5);
		dal[5] = 34729d / 80640d * Math.Pow(dn, 5);
		double dSg = 0; double dTu = 0;
		for (int j = 1; j <= 5; j++)
		{
			dSg = dSg + 2 * j * dal[j] * Math.Cos(2 * j * dXi) * Math.Cosh(2 * j * dEt);
			dTu = dTu + 2 * j * dal[j] * Math.Sin(2 * j * dXi) * Math.Sinh(2 * j * dEt);
		}
		dSg = 1 + dSg;

		//A0-A5
		double[] dA = new double[6];
		dA[0] = 1 + Math.Pow(dn, 2) / 4 + Math.Pow(dn, 4) / 64;
		dA[1] = -3d / 2d * (dn - Math.Pow(dn, 3) / 8 - Math.Pow(dn, 5) / 64);
		dA[2] = 15d / 16d * (Math.Pow(dn, 2) - Math.Pow(dn, 4) / 4);
		dA[3] = -35d / 48d * (Math.Pow(dn, 3) - 5d / 16d * Math.Pow(dn, 5));
		dA[4] = 315d / 512d * Math.Pow(dn, 4);
		dA[5] = -693d / 1280d * Math.Pow(dn, 5);
		double dAb = dM0 * daa / (1 + dn) * dA[0];
		double dSb = 0;
		for (int j = 1; j <= 5; j++)
		{
			dSb = dSb + dA[j] * Math.Sin(2 * j * Lat0);
		}
		dSb = dM0 * daa / (1 + dn) * (dA[0] * Lat0 + dSb);

		double Y = 0;
		double X = 0;
		for (int j = 1; j <= 5; j++)
		{
			Y = Y + dal[j] * Math.Sin(2 * j * dXi) * Math.Cosh(2 * j * dEt);
			X = X + dal[j] * Math.Cos(2 * j * dXi) * Math.Sinh(2 * j * dEt);
		}
		
		Y = dAb * (dXi + Y) - dSb;
		X = dAb * (dEt + X);
		
		// 真北方向角(分)を求める。(プラスは真北より西，マイナスは東)
		double del = 1.0d;
		double tau = 0.0d;
		for(int i=1;i<=5;i++){
			del = del + 2.0*((double)i)*dal[i]*Math.Cos(2.0*((double)i)*dXi)*Math.Cosh(2.0*((double)i)*dEt) ;
			tau = tau + 2.0*((double)i)*dal[i]*Math.Sin(2.0*((double)i)*dXi)*Math.Sinh(2.0*((double)i)*dEt) ;
		}	
		double lamc = Math.Cos(Lon-Lon0);
		double lams = Math.Sin(Lon-Lon0);
		double gam=-Math.Atan( (tau*dtb*lamc+del*dt*lams)/(del*dtb*lamc-tau*dt*lams) )/Math.PI;
		
		double [] array = new double [3] {X, Y, gam};
		return array;
	}

	// 平面直角座標（汎用）→経度緯度
	// double [経度(°),緯度(°)] = EN2LonLat(E座標(m), N座標(m), 基準点経度(°), 基準点緯度(°), 縮尺係数)
	public double[] EN2LonLat(double X, double Y, double Lon0, double Lat0, double dM0)
	{
		double dn = 1d / (2 * dF - 1);
		Lon0 = Deg2Rad(Lon0);
		Lat0 = Deg2Rad(Lat0);

		//Sφ0、A
		double[] dA = new double[6];
		dA[0] = 1 + Math.Pow(dn, 2) / 4 + Math.Pow(dn, 4) / 64;
		dA[1] = -3d / 2d * (dn - Math.Pow(dn, 3) / 8 - Math.Pow(dn, 5) / 64);
		dA[2] = 15d / 16d * (Math.Pow(dn, 2) - Math.Pow(dn, 4) / 4);
		dA[3] = -35d / 48d * (Math.Pow(dn, 3) - 5d / 16d * Math.Pow(dn, 5));
		dA[4] = 315d / 512d * Math.Pow(dn, 4);
		dA[5] = -693d / 1280d * Math.Pow(dn, 5);
		double dAb = dM0 * daa / (1 + dn) * dA[0];
		double dSb = 0;
		for (int j = 1; j <= 5; j++)
		{
			dSb = dSb + dA[j] * Math.Sin(2 * j * Lat0);
		}
		dSb = dM0 * daa / (1 + dn) * (dA[0] * Lat0 + dSb);

		//ξ・η
		double dXi = (Y + dSb) / dAb;
		double dEt = X / dAb;

		//β
		double[] dBt = new double[6];
		dBt[1] = 1d / 2d * dn - 2d / 3d * Math.Pow(dn, 2) + 37d / 96d * Math.Pow(dn, 3) - 1d / 360d * Math.Pow(dn, 4) - 81d / 512d * Math.Pow(dn, 5);
		dBt[2] = 1d / 48d * Math.Pow(dn, 2) + 1d / 15d * Math.Pow(dn, 3) - 437d / 1440d * Math.Pow(dn, 4) + 46d / 105d * Math.Pow(dn, 5);
		dBt[3] = 17d / 480d * Math.Pow(dn, 3) - 37d / 840d * Math.Pow(dn, 4) - 209d / 4480d * Math.Pow(dn, 5);
		dBt[4] = 4397d / 161280d * Math.Pow(dn, 4) - 11d / 504d * Math.Pow(dn, 5);
		dBt[5] = 4583d / 161280d * Math.Pow(dn, 5);

		//ξ’・η'・σ'・τ'・χ
		double dXi2 = 0;
		double dEt2 = 0;
		double dSg2 = 0;
		double dTu2 = 0;
		for (int j = 1; j <= 5; j++)
		{
			dXi2 = dXi2 + dBt[j] * Math.Sin(2 * j * dXi) * Math.Cosh(2 * j * dEt);
			dEt2 = dEt2 + dBt[j] * Math.Cos(2 * j * dXi) * Math.Sinh(2 * j * dEt);
			dSg2 = dSg2 + dBt[j] * Math.Cos(2 * j * dXi) * Math.Cosh(2 * j * dEt);
			dTu2 = dTu2 + dBt[j] * Math.Sin(2 * j * dXi) * Math.Sinh(2 * j * dEt);
		}
		dXi2 = dXi - dXi2;
		dEt2 = dEt - dEt2;
		dSg2 = 1 - dSg2;
		double dCi = Math.Asin(Math.Sin(dXi2) / Math.Cosh(dEt2));

		//δ
		double[] dDt = new double[7];
		dDt[1] = 2 * dn - 2d / 3d * Math.Pow(dn, 2) - 2 * Math.Pow(dn, 3) + 116d / 45d * Math.Pow(dn, 4) + 26d / 45d * Math.Pow(dn, 5) - 2854d / 675d * Math.Pow(dn, 6);
		dDt[2] = 7d / 3d * Math.Pow(dn, 2) - 8d / 5d * Math.Pow(dn, 3) - 227d / 45d * Math.Pow(dn, 4) + 2704d / 315d * Math.Pow(dn, 5) + 2323d / 945d * Math.Pow(dn, 6);
		dDt[3] = 56d / 15d * Math.Pow(dn, 3) - 136d / 35d * Math.Pow(dn, 4) - 1262d / 105d * Math.Pow(dn, 5) + 73814d / 2835d * Math.Pow(dn, 6);
		dDt[4] = 4279d / 630d * Math.Pow(dn, 4) - 332d / 35d * Math.Pow(dn, 5) - 399572d / 14175d * Math.Pow(dn, 6);
		dDt[5] = 4174d / 315d * Math.Pow(dn, 5) - 144838d / 6237d * Math.Pow(dn, 6);
		dDt[6] = 601676d / 22275d * Math.Pow(dn, 6);

		//ラジアン単位の緯度経度
		double Lon = Lon0 + Math.Atan(Math.Sinh(dEt2) / Math.Cos(dXi2));
		double Lat = dCi;
		for (int j = 1; j <= 6; j++)
		{
			Lat = Lat + dDt[j] * Math.Sin(2 * j * dCi);
		}

		//度単位に
		Lon = 180 * Lon / Math.PI;
		Lat = 180 * Lat / Math.PI;
		
		double [] array = new double [2] {Lon, Lat};
		return array;
	}

	// 緯度経度→平面直角座標（JP19系）
	// double [E座標(m), N座標(m), 真北偏差(°)] = LonLat2EN(経度(°), 緯度(°), 系番号)
	public double [] LonLat2JP(double Lon, double Lat, int Zone)
	{
		double [] EN=LonLat2EN(Lon, Lat, jp19list[Zone][0], jp19list[Zone][1], 0.9999d);
		return EN;
	}

	// 平面直角座標（JP19系）→緯度経度
	// double [経度(°), 緯度(°)] = EN2LonLat(E座標(m), N座標(m), 系番号)
	public double[] JP2LonLat(double X, double Y, int Zone)
	{
		double[] LL=EN2LonLat(X, Y, jp19list[Zone][0], jp19list[Zone][1], 0.9999d);
		return LL;
	}

	// 緯度経度→UTM
	// double [E座標(m), N座標(m), 真北偏差(°)] = LonLat2EN(経度(°), 緯度(°), ゾーン番号)
	public double [] LonLat2UTM(double Lon, double Lat, int Zone)
	{
		double center_lat = 0d;
		double center_lon= (Zone-30)*6-3;
		double [] EN=LonLat2EN(Lon, Lat, center_lon, center_lat, 0.9996d);
		EN[0] += 500000d;
		if(EN[1] < 0){
			EN[1]+= 10000000d;
		}
		return EN;
	}

	// 平面直角座標（UTM）→経度緯度
	// double [経度(°), 緯度(°)] = EN2LonLat(E座標(m), N座標(m), 系番号)
	public double[] UTM2LonLat(double X, double Y, int Zone)
	{
		double center_lat = 0d;
		double center_lon= (Zone-30)*6-3;
		if(Y>5000000d){
			Y -= 10000000d;
		}
		double[] LL=EN2LonLat(X - 500000d, Y, center_lon, center_lat, 0.9996d);
		return LL;
	}

	//双曲線正接関数の逆関数
	private static double atanh(double x)
	{
		return (1d / 2d * Math.Log((1 + x) / (1 - x), Math.E));
	}
	private static double Deg2Rad(double Deg)
	{
		return (Math.PI * Deg / 180d);
	}

	//直角座標x,yにおける中心原点からの下降量とE・N方向の傾き
	//double[下降量,E傾き,N傾き] = downXY(double x, double y)
	public double[] downXY(double x, double y){
		double eqEarth=1.156;			//等価地球半径（倍）
		double radEarth=6378.137;		//地球半径（km）：GRS80準拠楕円体
		double eqrad = radEarth*eqEarth*1000;
		double rh = Math.Sqrt(x*x+y*y);
		double[] down = new double [3] {(1-Math.Cos(Math.Asin(rh/eqrad)))*eqrad,Math.Asin(x/eqrad),Math.Asin(y/eqrad)};
		return down;
	}
	
	//座標系設定に応じて、入力座標をunity空間のX,Yに変換する
	public double[] UnityXY(double x, double y){
		double [] EN = new double[2];
		if(aAV_Public.center.type == "WG"){
			EN=LonLat2EN(x, y, aAV_Public.center.WGS_E, aAV_Public.center.WGS_N,1d);
		}else if(aAV_Public.center.type == "JP"){
			EN[0]=x-aAV_Public.center.JPRCS_E;
			EN[1]=y-aAV_Public.center.JPRCS_N;;
		}else if(aAV_Public.center.type == "UT"){
			EN[0]=x-aAV_Public.center.UTM_E;
			EN[1]=y-aAV_Public.center.UTM_N;
		}
		return EN;
	}

	//座標系設定に応じて、unity空間のX,Yを指定座標系X,Yに変換する
	public double[] TypeXY(double x, double y){
		double [] EN = new double[2];
		if(aAV_Public.center.type == "WG"){
			EN = EN2LonLat(x, y, aAV_Public.center.WGS_E, aAV_Public.center.WGS_N, 1d);
		}else if(aAV_Public.center.type == "JP"){
			EN[0]=x+aAV_Public.center.JPRCS_E;
			EN[1]=y+aAV_Public.center.JPRCS_N;
		}else if(aAV_Public.center.type == "UT"){
			EN[0]=x+aAV_Public.center.UTM_E;
			EN[1]=y+aAV_Public.center.UTM_N;
		}
		return EN;
	}

	public void CenterCalc(){
		double [] EN;
		if(aAV_Public.basicInfo.type == "WG"){
			aAV_Public.center.WGS_E = aAV_Public.basicInfo.center_E;
			aAV_Public.center.WGS_N= aAV_Public.basicInfo.center_N;
			EN = LonLat2UTM(aAV_Public.center.WGS_E, aAV_Public.center.WGS_N, aAV_Public.center.UTM_zone);
			aAV_Public.center.UTM_E = EN[0];
			aAV_Public.center.UTM_N = EN[1];
			EN = LonLat2JP(aAV_Public.center.WGS_E, aAV_Public.center.WGS_N, aAV_Public.center.JPRCS_zone);
			aAV_Public.center.JPRCS_E = EN[0];
			aAV_Public.center.JPRCS_N = EN[1];
		}else if(aAV_Public.basicInfo.type == "UT"){
			EN = UTM2LonLat(aAV_Public.basicInfo.center_E, aAV_Public.basicInfo.center_N, aAV_Public.basicInfo.zone);
			aAV_Public.center.WGS_E = EN[0];
			aAV_Public.center.WGS_N = EN[1];
			EN = LonLat2UTM(aAV_Public.center.WGS_E, aAV_Public.center.WGS_N, aAV_Public.center.UTM_zone);
			aAV_Public.center.UTM_E = EN[0];
			aAV_Public.center.UTM_N = EN[1];
			EN = LonLat2JP(aAV_Public.center.WGS_E, aAV_Public.center.WGS_N, aAV_Public.center.JPRCS_zone);
			aAV_Public.center.JPRCS_E = EN[0];
			aAV_Public.center.JPRCS_N = EN[1];
		}else if(aAV_Public.basicInfo.type == "JP"){
			EN = JP2LonLat(aAV_Public.basicInfo.center_E, aAV_Public.basicInfo.center_N, aAV_Public.basicInfo.zone);
			aAV_Public.center.WGS_E = EN[0];
			aAV_Public.center.WGS_N = EN[1];
			EN = LonLat2UTM(aAV_Public.center.WGS_E, aAV_Public.center.WGS_N, aAV_Public.center.UTM_zone);
			aAV_Public.center.UTM_E = EN[0];
			aAV_Public.center.UTM_N = EN[1];
			EN = LonLat2JP(aAV_Public.center.WGS_E, aAV_Public.center.WGS_N, aAV_Public.center.JPRCS_zone);
			aAV_Public.center.JPRCS_E = EN[0];
			aAV_Public.center.JPRCS_N = EN[1];
		}
		Debug.Log("WGS84="+aAV_Public.center.WGS_E+","+aAV_Public.center.WGS_N+", UTM"+aAV_Public.center.UTM_zone+"="+aAV_Public.center.UTM_E+","+aAV_Public.center.UTM_N+", JPRCS"+aAV_Public.center.JPRCS_zone+"="+aAV_Public.center.JPRCS_E+","+aAV_Public.center.JPRCS_N);
	}

}
