namespace :nug do
	@nuget = "lib/nuget.exe"
	
	@packname = 'bottles'
	@dependencies = ['FubuCore']
	@nugroot = File.expand_path("/nugs")
	
	desc "Build the nuget package"
	task :build do
		sh "#{@nuget} pack packaging/nuget/bottles.nuspec -o #{ARTIFACTS} -Version #{BUILD_NUMBER}"
	end

	
	desc "pulls new NuGet updates from your local machine"
	task :pull, [:location] do |t, args|
		args.with_defaults(:location => 'local')
		location = args[:location]
		
		@dependencies.each do |f|
			sh "#{@nuget} install #{f} /Source #{@nugroot} /ExcludeVersion /OutputDirectory .\\lib"
		end
	end
		
	desc "pushes new NuGet udates to your local machine"
	task :push, [:location] => [:build] do |t, args|
		args.with_defaults(:location => 'local')
		location = args[:location]
			
		Dir["#{ARTIFACTS}/*.nupkg"].each do |fn|		
			FileUtils.cp fn, @nugroot
		end
	end
	
end