# IEModeExpiryFix
IEModeExpiryFix - fix date expiration in edge`s list of IE11-mode pages

by default pages in list expires in 30 days

replaces in files

	C:\Users\<current user>\AppData\Local\Microsoft\Edge\User Data\*\Preferences


    {
	    "dual_engine":{
		    "user_list_data_1":{
			    "http(s)//site.com":{
				    "date_added": "13322850221511271" => "15741381600000000" //2099 year
			    }
		    }
	    }
    }
